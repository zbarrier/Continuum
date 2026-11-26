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

/// <summary>The Order Shipped Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IOrderShippedNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the client user who entered the order, with a link to the order, when the AMC ships its report, then records on
///     the order that the notice was sent, signed in as <see cref="AutomationUser.OrderShippedNotifier"/>.
/// </summary>
/// <remarks>It works like the <see cref="ReportDeliveredNotifierGrain"/>.</remarks>
[GrainType("orderShippedNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class OrderShippedNotifierGrain : Grain, IOrderShippedNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<OrderShippedNotifierGrain> _logger;

    public OrderShippedNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<OrderShippedNotifierGrain> logger)
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
        OrderShipped shipped => ReactToOrderShippedAsync(shipped),
        _ => Task.CompletedTask
    };

    /// <exception cref="OrderShippedNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToOrderShippedAsync(OrderShipped shipped)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(shipped.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, shipped.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.OrderShippedNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, shipped);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, OrderShipped shipped)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.UserId == shipped.EnteredByUserId);
        if (recipient is null)
        {
            LogNoRecipient(orderId, shipped.EnteredByUserId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsOrderShippedNoticeSentAsync(shipped.DocumentId);
        if (sent.IsFailure)
        {
            HandleFailure(orderId, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var shipper = users.FirstOrDefault(user => user.UserId == shipped.InitiatedBy);
        var amcName = shipper?.OrganizationName ?? "The AMC";
        var subject = shipped.OrderNumber is { } orderNumber
            ? $"Order {orderNumber}: your report is ready"
            : "Your order's report is ready";
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + $"<p>{WebUtility.HtmlEncode(amcName)} completed your order. You can download the report from the order.</p>";
        if (OrderUrl(orderId) is { } orderUrl)
        {
            body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Open the order</a></p>";
        }
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendOrderShippedNoticeAsync(new SendOrderShippedNoticeCommand(
            shipped.DocumentId, recipient.UserId, recipient.Email, subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, recorded.Error);
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

    private void HandleFailure(string orderId, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, error);
            throw new OrderShippedNoticeNotRecordedException(
                $"The notice of the shipped order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Order Shipped Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the shipped order {OrderId}: user {RecipientUserId} is not in the user directory.")]
    private partial void LogNoRecipient(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the shipped order {OrderId}: user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of the shipped order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of the shipped order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, Error error);
}
