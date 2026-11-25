using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Assigns the submitted order to an AMC staff member in the tenant's user directory (the order page's Assign to me
    ///     assigns it to the signed-in user). Only the Client Services Director or the tenant admin may assign orders, and an
    ///     order is assigned only once.
    /// </summary>
    Task<Result<AssignOrderResponse>> AssignOrderAsync(AssignOrderCommand command);
}

/// <param name="AssignedToUserId">The staff member to assign the order to, as a tenant user ID <c>{TenantId}|{UserGuid}</c>.</param>
[Alias("Orders.AssignOrderCommand.V1"), GenerateSerializer, Immutable]
public sealed record AssignOrderCommand(string AssignedToUserId);

[Alias("Orders.AssignOrderResponse.V1"), GenerateSerializer, Immutable]
public sealed class AssignOrderResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The order is assigned to a client services user (status Assigned).</summary>
[Alias("Orders.OrderAssigned.V1"), DomainEventType("Orders.OrderAssigned.V1"), GenerateSerializer, Immutable]
public sealed class OrderAssigned : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string AssignedToUserId { get; init; }
}
