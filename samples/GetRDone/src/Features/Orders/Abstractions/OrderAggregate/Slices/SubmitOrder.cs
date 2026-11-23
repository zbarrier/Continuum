using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Submits the draft (order entry tab 4, Review &amp; Submit). The draft needs a borrower; supporting documents are
    ///     optional. Once submitted, the order can no longer be changed as a draft.
    /// </summary>
    Task<Result<SubmitOrderResponse>> SubmitOrderAsync();
}

[Alias("Orders.SubmitOrderResponse.V1"), GenerateSerializer, Immutable]
public sealed class SubmitOrderResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The order is submitted to the AMC (status Submitted), for the client the draft was started for.</summary>
[Alias("Orders.OrderSubmitted.V1"), DomainEventType("Orders.OrderSubmitted.V1"), GenerateSerializer, Immutable]
public sealed class OrderSubmitted : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string ClientId { get; init; }
}
