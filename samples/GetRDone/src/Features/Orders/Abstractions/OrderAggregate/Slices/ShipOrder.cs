using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Ships the vendor's report to the client once it is delivered, which completes the order. The staff member the order
    ///     is assigned to, the Client Services Director or the tenant admin may ship it, once; the Order Shipped Notifier then
    ///     tells the client user who entered the order.
    /// </summary>
    Task<Result<ShipOrderResponse>> ShipOrderAsync(ShipOrderCommand command);
}

/// <summary>The report the AMC ships, which must be the one the vendor delivered.</summary>
[Alias("Orders.ShipOrderCommand.V1"), GenerateSerializer, Immutable]
public sealed record ShipOrderCommand(string DocumentId);

[Alias("Orders.ShipOrderResponse.V1"), GenerateSerializer, Immutable]
public sealed class ShipOrderResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The AMC shipped the report to the client; <see cref="DomainEvent.OccurredAt"/> is when it shipped.</summary>
/// <remarks>
///     It carries the client user who entered the order and the order's number, when it has one, so the Order Shipped
///     Notifier can tell them without reading the order.
/// </remarks>
[Alias("Orders.OrderShipped.V1"), DomainEventType("Orders.OrderShipped.V1"), GenerateSerializer, Immutable]
public sealed class OrderShipped : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }

    /// <summary>The report the client gets.</summary>
    [Id(1)] public required string DocumentId { get; init; }

    /// <summary>The client user who entered the order, who is told the report is ready.</summary>
    [Id(2)] public required string EnteredByUserId { get; init; }

    [Id(3)] public long? OrderNumber { get; init; }
}
