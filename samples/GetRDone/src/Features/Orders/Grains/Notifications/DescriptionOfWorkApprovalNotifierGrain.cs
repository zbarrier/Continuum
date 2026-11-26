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

/// <summary>The Description of Work Approval Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IDescriptionOfWorkApprovalNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the client who approves the order's description of work a link to the order when a version is submitted for
///     their approval, then records on the order that the notice was sent, signed in as
///     <see cref="AutomationUser.DescriptionOfWorkApprovalNotifier"/>.
/// </summary>
/// <remarks>
///     It works like the <see cref="DocumentRequestNotifierGrain"/>. It asks the order whether the version's notice is due
///     before it emails, so a redelivered submission isn't emailed twice, and a version that was revised or approved
///     since isn't emailed at all.
/// </remarks>
[GrainType("descriptionOfWorkApprovalNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class DescriptionOfWorkApprovalNotifierGrain : Grain, IDescriptionOfWorkApprovalNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<DescriptionOfWorkApprovalNotifierGrain> _logger;

    public DescriptionOfWorkApprovalNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<DescriptionOfWorkApprovalNotifierGrain> logger)
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
        DescriptionOfWorkSubmitted submitted => ReactToDescriptionOfWorkSubmittedAsync(submitted),
        _ => Task.CompletedTask
    };

    /// <exception cref="DescriptionOfWorkSubmittedNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToDescriptionOfWorkSubmittedAsync(DescriptionOfWorkSubmitted submitted)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(submitted.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, submitted.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.DescriptionOfWorkApprovalNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, submitted);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, DescriptionOfWorkSubmitted submitted)
    {
        var version = submitted.DescriptionOfWorkVersion;
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.UserId == submitted.ApproverUserId);
        if (recipient is null)
        {
            LogNoRecipient(orderId, version, submitted.ApproverUserId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, version, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var due = await order.IsDescriptionOfWorkSubmittedNoticeDueAsync(submitted.DescriptionOfWorkId, version);
        if (due.IsFailure)
        {
            HandleFailure(orderId, version, due.Error);
            return;
        }
        if (!due.Value)
        {
            return;
        }

        var subject = submitted.OrderNumber is { } orderNumber
            ? $"Order {orderNumber}: please approve the description of work"
            : "Your order: please approve the description of work";
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + (version == DescriptionOfWorkVersion.MinValue
                ? "<p>The description of work for your order is ready for your approval.</p>"
                : $"<p>The description of work for your order was revised (version {version}) and is ready for your approval.</p>");
        if (OrderUrl(orderId) is { } orderUrl)
        {
            body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Review the description of work</a></p>";
        }
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendDescriptionOfWorkSubmittedNoticeAsync(new SendDescriptionOfWorkSubmittedNoticeCommand(
            submitted.DescriptionOfWorkId, version, recipient.UserId, recipient.Email, subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, version, recorded.Error);
        }
    }

    /// <summary>The client order page in GetRDone.Web, or null when the web app's URL isn't configured.</summary>
    private string? OrderUrl(string orderId)
    {
        if (string.IsNullOrWhiteSpace(_options.WebBaseUrl))
        {
            return null;
        }
        TenantKey.Split(orderId, out _, out var orderGuid);
        return $"{_options.WebBaseUrl.TrimEnd('/')}/orders/{orderGuid}";
    }

    private void HandleFailure(string orderId, int version, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, version, error);
            throw new DescriptionOfWorkSubmittedNoticeNotRecordedException(
                $"The notice of version {version} of the description of work of order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, version, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Description of Work Approval Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for version {Version} of the description of work of order {OrderId}: user {RecipientUserId} is not in the user directory.")]
    private partial void LogNoRecipient(string orderId, int version, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for version {Version} of the description of work of order {OrderId}: user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, int version, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of version {Version} of the description of work of order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, int version, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of version {Version} of the description of work of order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, int version, Error error);
}
