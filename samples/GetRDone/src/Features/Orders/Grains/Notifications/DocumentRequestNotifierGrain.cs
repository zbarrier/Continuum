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

/// <summary>The Document Request Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IDocumentRequestNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the client the AMC requested documents from, with a link to the order, then records on the order that the
///     notice was sent, signed in as <see cref="AutomationUser.DocumentRequestNotifier"/>.
/// </summary>
/// <remarks>
///     It works like the <see cref="NewOrderNotifierGrain"/>: the <see cref="NotificationReactors"/> stream provider
///     delivers each order's events to it keyed by the order ID, it asks the order whether the notice was sent before it
///     emails, and it throws for a failure that may pass so the event is delivered again. The link uses
///     <see cref="NotificationOptions.WebBaseUrl"/>.
/// </remarks>
[GrainType("documentRequestNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class DocumentRequestNotifierGrain : Grain, IDocumentRequestNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<DocumentRequestNotifierGrain> _logger;

    public DocumentRequestNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<DocumentRequestNotifierGrain> logger)
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
        ClientDocumentRequested requested => ReactToClientDocumentRequestedAsync(requested),
        _ => Task.CompletedTask
    };

    /// <exception cref="DocumentRequestNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToClientDocumentRequestedAsync(ClientDocumentRequested requested)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(requested.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, requested.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.DocumentRequestNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, requested);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, ClientDocumentRequested requested)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.UserId == requested.RequestedFromUserId);
        if (recipient is null)
        {
            LogNoRecipient(orderId, requested.RequestId, requested.RequestedFromUserId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, requested.RequestId, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsDocumentRequestNoticeSentAsync(requested.RequestId);
        if (sent.IsFailure)
        {
            HandleFailure(orderId, requested.RequestId, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var subject = requested.OrderNumber is { } orderNumber
            ? $"Order {orderNumber}: please upload a {requested.DocumentTypeName}"
            : $"Your order: please upload a {requested.DocumentTypeName}";
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + $"<p>To keep your order moving, please upload a <strong>{WebUtility.HtmlEncode(requested.DocumentTypeName)}</strong>.</p>"
            + $"<p>{WebUtility.HtmlEncode(requested.Message)}</p>";
        if (OrderUrl(orderId) is { } orderUrl)
        {
            body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Open the order</a></p>";
        }
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendDocumentRequestNoticeAsync(
            new SendDocumentRequestNoticeCommand(requested.RequestId, recipient.UserId, recipient.Email, subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, requested.RequestId, recorded.Error);
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

    private void HandleFailure(string orderId, string requestId, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, requestId, error);
            throw new DocumentRequestNoticeNotRecordedException(
                $"The document request notice {requestId} for order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, requestId, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Document Request Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for document request {RequestId} of order {OrderId}: user {RecipientUserId} is not in the user directory.")]
    private partial void LogNoRecipient(string orderId, string requestId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for document request {RequestId} of order {OrderId}: user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, string requestId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of document request {RequestId} of order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, string requestId, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of document request {RequestId} of order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, string requestId, Error error);
}
