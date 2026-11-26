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

/// <summary>The Description of Work Approved Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IDescriptionOfWorkApprovedNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the staff member the order is assigned to, with a link to the order, when the client approves the order's
///     description of work, then records on the order that the notice was sent, signed in as
///     <see cref="AutomationUser.DescriptionOfWorkApprovedNotifier"/>.
/// </summary>
/// <remarks>
///     It works like the <see cref="DocumentRequestNotifierGrain"/>, linking to the staff order page instead of the
///     client's.
/// </remarks>
[GrainType("descriptionOfWorkApprovedNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class DescriptionOfWorkApprovedNotifierGrain : Grain, IDescriptionOfWorkApprovedNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<DescriptionOfWorkApprovedNotifierGrain> _logger;

    public DescriptionOfWorkApprovedNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<DescriptionOfWorkApprovedNotifierGrain> logger)
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
        DescriptionOfWorkApproved approved => ReactToDescriptionOfWorkApprovedAsync(approved),
        _ => Task.CompletedTask
    };

    /// <exception cref="DescriptionOfWorkApprovedNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToDescriptionOfWorkApprovedAsync(DescriptionOfWorkApproved approved)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(approved.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, approved.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.DescriptionOfWorkApprovedNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, approved);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, DescriptionOfWorkApproved approved)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.UserId == approved.AssignedToUserId);
        if (recipient is null)
        {
            LogNoRecipient(orderId, approved.AssignedToUserId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsDescriptionOfWorkApprovedNoticeSentAsync(approved.DescriptionOfWorkId);
        if (sent.IsFailure)
        {
            HandleFailure(orderId, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var approverName = users.FirstOrDefault(user => user.UserId == approved.InitiatedBy)?.DisplayName ?? "The client";
        var subject = approved.OrderNumber is { } orderNumber
            ? $"Order {orderNumber}: the description of work was approved"
            : "An order's description of work was approved";
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + $"<p>{WebUtility.HtmlEncode(approverName)} approved version {approved.DescriptionOfWorkVersion} of the description of work.</p>";
        if (OrderUrl(orderId) is { } orderUrl)
        {
            body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Open the order</a></p>";
        }
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendDescriptionOfWorkApprovedNoticeAsync(new SendDescriptionOfWorkApprovedNoticeCommand(
            approved.DescriptionOfWorkId, recipient.UserId, recipient.Email, subject));
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
            throw new DescriptionOfWorkApprovedNoticeNotRecordedException(
                $"The notice of the approved description of work of order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Description of Work Approved Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the approved description of work of order {OrderId}: user {RecipientUserId} is not in the user directory.")]
    private partial void LogNoRecipient(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the approved description of work of order {OrderId}: user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of the approved description of work of order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of the approved description of work of order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, Error error);
}
