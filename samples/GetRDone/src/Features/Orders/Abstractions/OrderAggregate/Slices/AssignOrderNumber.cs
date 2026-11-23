using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Gives the submitted order the number the order number generator issued it. Only the Order Number Assigner
    ///     automation may assign it. Succeeds without changes when the order already has a number, so the automation can
    ///     safely retry.
    /// </summary>
    Task<Result<AssignOrderNumberResponse>> AssignOrderNumberAsync(long orderNumber);
}

[Alias("Orders.AssignOrderNumberResponse.V1"), GenerateSerializer, Immutable]
public sealed class AssignOrderNumberResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The submitted order has its order number, which the client and the AMC know it by from now on.</summary>
[Alias("Orders.OrderNumberAssigned.V1"), DomainEventType("Orders.OrderNumberAssigned.V1"), GenerateSerializer, Immutable]
public sealed class OrderNumberAssigned : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required long OrderNumber { get; init; }
    [Id(2)] public required string ClientId { get; init; }
}
