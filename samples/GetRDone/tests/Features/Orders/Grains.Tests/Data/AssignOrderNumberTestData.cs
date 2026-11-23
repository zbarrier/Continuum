using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>
///     Order number assignments: the events that give the <see cref="SubmitOrderTestData"/> orders their numbers, each
///     by the Order Number Assigner automation, for the client the draft was started for.
/// </summary>
public static class AssignOrderNumberTestData
{
    /// <summary>The first number a tenant's generator issues.</summary>
    public const long FirstOrderNumber = 610_696;

    /// <summary>The number assignment of the order <c>Submitted1</c> submitted.</summary>
    public static OrderNumberAssigned Assigned1(string orderId, long orderNumber = FirstOrderNumber, DateTimeOffset? occurredAt = null)
        => Assigned(StartOrderDraftTestData.Command1, orderId, orderNumber, occurredAt);

    /// <summary>The event the order grain records when the automation gives the order <paramref name="draft"/> started its number.</summary>
    public static OrderNumberAssigned Assigned(
        StartOrderDraftCommand draft, string orderId, long orderNumber, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        OrderNumber = orderNumber,
        ClientId = draft.ClientId,
        InitiatedBy = AutomationUser.OrderNumberAssigner.GetTenantUserId(TestData.TenantId),
        OccurredAt = occurredAt ?? TestData.At
    };
}
