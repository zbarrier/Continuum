using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     New order notices: the notice the New Order Notifier sends the test tenant's Client Services Director,
///     <see cref="TestData.Director1"/>, about an order <see cref="AssignOrderNumberTestData"/> gave its number.
/// </summary>
public static class SendNewOrderNoticeTestData
{
    /// <summary>The notice about the order <c>Assigned1</c> gave its first number, for John Doe's client.</summary>
    public static SendNewOrderNoticeCommand Command1 { get; } = new(
        TestData.Director1.UserId,
        TestData.Director1.Email,
        Subject(AssignOrderNumberTestData.FirstOrderNumber));

    /// <summary>The subject the notifier gives the notice about an order of Lakeview Credit Union, John Doe's client.</summary>
    public static string Subject(long orderNumber) => $"New order {orderNumber} from Lakeview Credit Union";

    /// <summary>The event the order grain records when the automation records <see cref="Command1"/>.</summary>
    public static NewOrderNoticeSent Sent1(
        string orderId, long orderNumber = AssignOrderNumberTestData.FirstOrderNumber, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        OrderNumber = orderNumber,
        RecipientUserId = Command1.RecipientUserId,
        RecipientEmail = Command1.RecipientEmail,
        Subject = Subject(orderNumber),
        InitiatedBy = AutomationUser.NewOrderNotifier.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
