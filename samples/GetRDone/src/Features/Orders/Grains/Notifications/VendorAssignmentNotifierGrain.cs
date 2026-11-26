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

/// <summary>The Vendor Assignment Notifier of one order. It has no methods; the stream provider activates it.</summary>
public interface IVendorAssignmentNotifierGrain : IGrainWithStringKey;

/// <summary>
///     Emails the users of the vendor an order is assigned to, with a link to the vendor order page, then records on the
///     order that the notice was sent, signed in as <see cref="AutomationUser.VendorAssignmentNotifier"/>.
/// </summary>
/// <remarks>
///     It works like the <see cref="DescriptionOfWorkApprovedNotifierGrain"/>, sending one email to each of the vendor's
///     users and recording them all in one notice.
/// </remarks>
[GrainType("vendorAssignmentNotifier")]
[ImplicitStreamSubscription(NotificationReactors.OrderNamespace)]
public sealed partial class VendorAssignmentNotifierGrain : Grain, IVendorAssignmentNotifierGrain, IStreamSubscriptionObserver
{
    private readonly IEmailSender _emailSender;
    private readonly NotificationOptions _options;
    private readonly ILogger<VendorAssignmentNotifierGrain> _logger;

    public VendorAssignmentNotifierGrain(
        IEmailSender emailSender, IOptions<NotificationOptions> options, ILogger<VendorAssignmentNotifierGrain> logger)
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
        VendorAssigned assigned => ReactToVendorAssignedAsync(assigned),
        _ => Task.CompletedTask
    };

    /// <exception cref="VendorAssignedNoticeNotRecordedException">The notice was not recorded for a reason that may pass.</exception>
    private async Task ReactToVendorAssignedAsync(VendorAssigned assigned)
    {
        var orderId = this.GetPrimaryKeyString();
        TenantKey.Split(orderId, out var tenantId, out _);
        if (tenantId is null || !string.Equals(assigned.OrderId, orderId, StringComparison.Ordinal))
        {
            LogEventForAnotherOrder(orderId, assigned.OrderId);
            return;
        }

        RequestContext.Set(OrleansIdentityConstants.USER_CLAIMS, AutomationUser.VendorAssignmentNotifier.CreatePrincipal(tenantId));
        try
        {
            await SendNoticeAsync(tenantId, orderId, assigned);
        }
        finally
        {
            RequestContext.Remove(OrleansIdentityConstants.USER_CLAIMS);
        }
    }

    private async Task SendNoticeAsync(string tenantId, string orderId, VendorAssigned assigned)
    {
        var users = await GrainFactory.GetUserDirectoryGrain(tenantId).GetUsersAsync();
        var recipients = users
            .Where(user => user.VendorId == assigned.VendorId && EmailAddress.Create(user.Email).IsSuccess)
            .ToArray();
        if (recipients.Length == 0)
        {
            LogNoRecipients(orderId, assigned.VendorId);
            return;
        }

        var order = GrainFactory.GetGrain<IOrderGrain>(orderId);
        var sent = await order.IsVendorAssignedNoticeSentAsync(assigned.VendorId);
        if (sent.IsFailure)
        {
            HandleFailure(orderId, sent.Error);
            return;
        }
        if (sent.Value)
        {
            return;
        }

        var assignerOrganization = users.FirstOrDefault(user => user.UserId == assigned.InitiatedBy)?.OrganizationName ?? "the AMC";
        var subject = assigned.OrderNumber is { } orderNumber
            ? $"New order {orderNumber} from {assignerOrganization}"
            : $"A new order from {assignerOrganization}";
        foreach (var recipient in recipients)
        {
            var body = $"<p>Hi {WebUtility.HtmlEncode(recipient.DisplayName)},</p>"
                + $"<p>{WebUtility.HtmlEncode(assignerOrganization)} assigned an order to you, due "
                + $"{WebUtility.HtmlEncode(assigned.VendorDueDate.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture))}.</p>";
            if (OrderUrl(orderId) is { } orderUrl)
            {
                body += $"<p><a href=\"{WebUtility.HtmlEncode(orderUrl)}\">Open the order</a></p>";
            }
            await _emailSender.SendAsync(new EmailMessage(recipient.Email, subject, body));
        }

        var recorded = await order.SendVendorAssignedNoticeAsync(new SendVendorAssignedNoticeCommand(
            assigned.VendorId,
            recipients.Select(recipient => new VendorNoticeRecipient(recipient.UserId, recipient.Email)).ToArray(),
            subject));
        if (recorded.IsFailure)
        {
            HandleFailure(orderId, recorded.Error);
        }
    }

    /// <summary>The vendor order page in GetRDone.Web, or null when the web app's URL isn't configured.</summary>
    private string? OrderUrl(string orderId)
    {
        if (string.IsNullOrWhiteSpace(_options.WebBaseUrl))
        {
            return null;
        }
        TenantKey.Split(orderId, out _, out var orderGuid);
        return $"{_options.WebBaseUrl.TrimEnd('/')}/vendor/orders/{orderGuid}";
    }

    private void HandleFailure(string orderId, Error error)
    {
        if (AutomationErrors.IsRetryable(error))
        {
            LogNoticeNotRecordedRetrying(orderId, error);
            throw new VendorAssignedNoticeNotRecordedException(
                $"The notice of the vendor assignment of order {orderId} was not recorded and will be retried.");
        }
        LogNoticeNotRecordedSkipped(orderId, error);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The Vendor Assignment Notifier of order {OrderId} ignored an event for order {EventOrderId}.")]
    private partial void LogEventForAnotherOrder(string orderId, string eventOrderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "No notice was sent for the vendor assignment of order {OrderId}: vendor {VendorId} has no users with a valid email address.")]
    private partial void LogNoRecipients(string orderId, string vendorId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The notice of the vendor assignment of order {OrderId} failed and will be retried: {Error}")]
    private partial void LogNoticeNotRecordedRetrying(string orderId, Error error);

    [LoggerMessage(Level = LogLevel.Error, Message = "The notice of the vendor assignment of order {OrderId} can never be recorded and is skipped: {Error}")]
    private partial void LogNoticeNotRecordedSkipped(string orderId, Error error);
}
