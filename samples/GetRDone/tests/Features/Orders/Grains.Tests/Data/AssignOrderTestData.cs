using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>Order assignments: the events that assign the <see cref="SubmitOrderTestData"/> orders to the AMC's staff.</summary>
public static class AssignOrderTestData
{
    /// <summary>Director1 assigns the order to themselves, as Assign to me does.</summary>
    public static OrderAssigned OrderAssigned1(string orderId, DateTimeOffset? occurredAt = null)
        => OrderAssigned(orderId, RequestedByUsers.Director1, RequestedByUsers.Director1, occurredAt);

    /// <summary>The event the order grain records when <paramref name="initiatedBy"/> assigns the order to <paramref name="assignedTo"/>.</summary>
    public static OrderAssigned OrderAssigned(
        string orderId, ClaimsPrincipal assignedTo, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        AssignedToUserId = assignedTo.GetTenantUserId(),
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
