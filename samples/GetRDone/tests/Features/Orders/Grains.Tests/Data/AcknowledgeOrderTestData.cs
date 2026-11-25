using System.Security.Claims;

using GetRDone.Orders.Abstractions.OrderAggregate;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>Order acknowledgements: the events that record the assigned staff member working on an order.</summary>
public static class AcknowledgeOrderTestData
{
    /// <summary>Director1 acknowledges the order <c>OrderAssigned1</c> assigned to them.</summary>
    public static OrderAcknowledged Acknowledged1(string orderId, DateTimeOffset? occurredAt = null)
        => Acknowledged(orderId, RequestedByUsers.Director1, occurredAt);

    /// <summary>The event the order grain records when <paramref name="initiatedBy"/> acknowledges the order.</summary>
    public static OrderAcknowledged Acknowledged(string orderId, ClaimsPrincipal initiatedBy, DateTimeOffset? occurredAt = null) => new()
    {
        OrderId = orderId,
        InitiatedBy = initiatedBy.GetTenantUserId(),
        OccurredAt = occurredAt ?? TestData.At
    };
}
