using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Shipping: once VendorUser1 delivers the report, Director1, whom the order is assigned to, ships it, and the Order
///     Shipped Notifier emails ClientUser1, who entered the order. The events follow from the commands, so they never drift
///     apart.
/// </summary>
public static class ShippingTestData
{
    /// <summary>Director1 ships the report VendorUser1 delivered.</summary>
    public static ShipOrderCommand ShipCommand1 { get; } = new(InspectionTestData.DeliverCommand1.DocumentId);

    /// <summary>The event the order grain records when Director1 sends <see cref="ShipCommand1"/> for ClientUser1's order.</summary>
    public static OrderShipped Shipped1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = ShipCommand1.DocumentId,
        EnteredByUserId = RequestedByUsers.ClientUser1.GetTenantUserId(),
        OrderNumber = orderNumber,
        InitiatedBy = RequestedByUsers.Director1.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };

    /// <summary>The subject the Order Shipped Notifier gives its notice.</summary>
    public static string NoticeSubject(long? orderNumber) => orderNumber is { } number
        ? $"Order {number}: your report is ready"
        : "Your order's report is ready";

    /// <summary>The notice the Order Shipped Notifier records after emailing ClientUser1.</summary>
    public static SendOrderShippedNoticeCommand NoticeCommand1(long? orderNumber = null) => new(
        ShipCommand1.DocumentId,
        TestData.ClientUser1.UserId,
        TestData.ClientUser1.Email,
        NoticeSubject(orderNumber));

    /// <summary>The event the order grain records when the automation records <see cref="NoticeCommand1"/>.</summary>
    public static OrderShippedNoticeSent NoticeSent1(string orderId, long? orderNumber = null, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        DocumentId = ShipCommand1.DocumentId,
        RecipientUserId = TestData.ClientUser1.UserId,
        RecipientEmail = TestData.ClientUser1.Email,
        Subject = NoticeSubject(orderNumber),
        InitiatedBy = AutomationUser.OrderShippedNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
