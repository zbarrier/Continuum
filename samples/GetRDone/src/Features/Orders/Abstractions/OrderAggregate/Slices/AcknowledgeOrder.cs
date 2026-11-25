using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     The staff member the order is assigned to confirms they are working on it. Rejected before the order is assigned
    ///     and once it is acknowledged.
    /// </summary>
    Task<Result<AcknowledgeOrderResponse>> AcknowledgeOrderAsync();
}

[Alias("Orders.AcknowledgeOrderResponse.V1"), GenerateSerializer, Immutable]
public sealed class AcknowledgeOrderResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The assigned user is working on the order (status Acknowledged).</summary>
[Alias("Orders.OrderAcknowledged.V1"), DomainEventType("Orders.OrderAcknowledged.V1"), GenerateSerializer, Immutable]
public sealed class OrderAcknowledged : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
}
