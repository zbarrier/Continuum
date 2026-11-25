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

using Orleans.Streams;
using Orleans.Streams.Core;

namespace GetRDone.Orders.Grains.Notifications;

/// <summary>The New Order Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface INewOrderNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the AMC staff member who assigns orders when an order is given its number, then records on the order that
///     the notice was sent, signed in as <see cref="AutomationUser.NewOrderNotifier"/>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="NotificationReactors"/> stream provider delivers each order's events to the notifier keyed by
///         the order ID. It runs apart from the order grain so sending an email never holds up the order.
///     </para>
///     <para>
///         The notifier asks the order whether the notice was sent before it emails, so an event delivered again (such as
///         after a silo restarts before the checkpoint is saved) doesn't email again. An email that went out just before
///         the notifier failed to record it is sent again when the event is delivered again; the sample accepts that rare
///         duplicate rather than missing a notice.
///     </para>
///     <para>
///         A failure that may pass (see <see cref="AutomationErrors.IsRetryable"/>), or an email that could not be sent,
///         throws, so the event is delivered again until the notice is recorded; one that never will, such as a tenant with
///         no one to tell, is logged and skipped.
///     </para>
/// </remarks>
[GrainType("newOrderNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class NewOrderNotifierGrain : Grain, INewOrderNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NewOrderNotifierGrain> _logger;

    public NewOrderNotifierGrain(IEmailSender emailSender, ILogger<NewOrderNotifierGrain> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>Attaches to the subscription the runtime created for the order's notifications.</summary>
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
        => await handleFactory.Create<IStreamedEvent<object>>().ResumeAsync((streamedEvent, _) => ReactAsync(streamedEvent.Event));

    private Task ReactAsync(object @event) => @event switch
    {
        OrderNumberAssigned assigned => ReactToOrderNumberAssignedAsync(assigned),
        _ => Task.CompletedTask
    };

    /// <exception cref="NewOrderNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToOrderNumberAssignedAsync(OrderNumberAssigned assigned)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(assigned.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, assigned.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.NewOrderNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, assigned);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, OrderNumberAssigned assigned)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipient = users.FirstOrDefault(user => user.Title == UserDirectoryEntry.ClientServicesDirectorTitle);
        if (recipient is null)
        {
            LogNoRecipient(orderId, assigned.OrderNumber, tenantId);
            return;
        }
        if (EmailAddress.Create(recipient.Email).IsFailure)
        {
            LogInvalidRecipientEmail(orderId, assigned.OrderNumber, recipient.UserId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsNewOrderNoticeSentAsync();
        if (sent.IsFailure)
        {
            HandleFailure(orderId, assigned.OrderNumber, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var clientName = users.FirstOrDefault(user => user.ClientId == assigned.ClientId)?.OrganizationName;
        var subject = clientName is null
            ? $"New order {assigned.OrderNumber}"
            : $"New order {assigned.OrderNumber} from {clientName}";
        var from = clientName is null ? "A client" : WebUtility.HtmlEncode(clientName);
        var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
            + $"<p>{from} submitted order <strong>{assigned.OrderNumber}</strong>. It is ready to be assigned.</p>";
        await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));

        var recorded = await order.SendNewOrderNoticeAsync(new SendNewOrderNoticeCommand(recipient.UserId, recipient.Email, subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, assigned.OrderNumber, recorded.Error);
        }
    }

    private void HandleFailure(string orderId, long orderNumber, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, orderNumber, error);
            throw new NewOrderNoticeNotRecordedException($"The new order notice for order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, orderNumber, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The New Order Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No new order notice was sent for order {OrderId} ({OrderNumber}): tenant {TenantId} has no one titled Client Services Director.")]
    private partial void LogNoRecipient(string orderId, long orderNumber, string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No new order notice was sent for order {OrderId} ({OrderNumber}): user {RecipientUserId} has no valid email address.")]
    private partial void LogInvalidRecipientEmail(string orderId, long orderNumber, string recipientUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The new order notice for order {OrderId} ({OrderNumber}) failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, long orderNumber, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The new order notice for order {OrderId} ({OrderNumber}) can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, long orderNumber, Error error);
}
