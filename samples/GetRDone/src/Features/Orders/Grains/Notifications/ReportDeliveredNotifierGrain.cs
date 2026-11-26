using System.Net;

using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;
using Continuum.Streaming;

using GetRDone.Email;
using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;
using GetRDone.Orders.Abstractions.ReadModels;

using ManagedCode.Orleans.Identity.Core.Constants;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Streams;
using Orleans.Streams.Core;

namespace GetRDone.Orders.Grains.Notifications;

/// <summary>The Report Delivered Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IReportDeliveredNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the staff member the order is assigned to, with a link to the order, when the vendor delivers its report, then
///     records on the order that the notice was sent, signed in as <see cref="AutomationUser.ReportDeliveredNotifier"/>.
/// </summary>
/// <remarks>It works like the <see cref="DescriptionOfWorkApprovedNotifierGrain"/>.</remarks>
[GrainType("reportDeliveredNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class ReportDeliveredNotifierGrain : Grain, IReportDeliveredNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<ReportDeliveredNotifierGrain> _logger;

    public ReportDeliveredNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<ReportDeliveredNotifierGrain> logger)
    {
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Attaches to the subscription the runtime created for the order's notifications.</summary>
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
        => await handleFactory.Create<IStreamedEvent<object>>().ResumeAsync((streamedEvent, _) => ReactAsync(streamedEvent.Event));

    private Task ReactAsync(object @event) => @event switch
    {
        ReportDelivered delivered => ReactToReportDeliveredAsync(delivered),
        _ => Task.CompletedTask
    };

    /// <exception cref="ReportDeliveredNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToReportDeliveredAsync(ReportDelivered delivered)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(delivered.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, delivered.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.ReportDeliveredNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, delivered);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, ReportDelivered delivered)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.UserId == delivered.AssignedToUserId);
        if (recipient is null)
        {
            LogNoRecipient(orderId, delivered.AssignedToUserId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsReportDeliveredNoticeSentAsync(delivered.DocumentId);
        if (sent.IsFailure)
        {
            HandleFailure(orderId, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var deliverer = users.FirstOrDefault(user => user.UserId == delivered.InitiatedBy);
        var delivererName = deliverer is null ? "The vendor" : $"{deliverer.DisplayName} of {deliverer.OrganizationName}";
        var subject = delivered.OrderNumber is { } orderNumber
            ? $"Order {orderNumber}: the report was delivered"
            : "An order's report was delivered";
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + $"<p>{WebUtility.HtmlEncode(delivererName)} delivered the report, {WebUtility.HtmlEncode(delivered.FileName)}, for review.</p>";
        if (OrderUrl(orderId) is { } orderUrl)
        {
            body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Open the order</a></p>";
        }
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendReportDeliveredNoticeAsync(new SendReportDeliveredNoticeCommand(
            delivered.DocumentId, recipient.UserId, recipient.Email, subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, recorded.Error);
        }
    }

    /// <summary>The staff order page in GetRDone.Web, or null when the web app's URL isn't configured.</summary>
    private string? OrderUrl(string orderId)
    {
        if (string.IsNullOrWhiteSpace(_options.WebBaseUrl))
        {
            return null;
        }
        TenantKey.Split(orderId, out _, out var orderGuid);
        return $"{_options.WebBaseUrl.TrimEnd('/')}/staff/orders/{orderGuid}";
    }

    private void HandleFailure(string orderId, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, error);
            throw new ReportDeliveredNoticeNotRecordedException(
                $"The notice of the delivered report of order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Report Delivered Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the delivered report of order {OrderId}: user {RecipientUserId} is not in the user directory.")]
    private partial void LogNoRecipient(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the delivered report of order {OrderId}: user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of the delivered report of order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of the delivered report of order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, Error error);
}
