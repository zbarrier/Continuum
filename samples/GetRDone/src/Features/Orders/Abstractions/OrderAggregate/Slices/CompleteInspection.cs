using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the appraiser inspected the property, once the inspection is scheduled. Only users of the vendor the
    ///     order is assigned to may record it, once.
    /// </summary>
    Task<Result<CompleteInspectionResponse>> CompleteInspectionAsync(CompleteInspectionCommand command);
}

/// <summary>The date the appraiser inspected the property, which can't be in the future.</summary>
[Alias("Orders.CompleteInspectionCommand.V1"), GenerateSerializer, Immutable]
public sealed record CompleteInspectionCommand(DateOnly CompletedOn);

[Alias("Orders.CompleteInspectionResponse.V1"), GenerateSerializer, Immutable]
public sealed class CompleteInspectionResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The vendor's appraiser inspected the property.</summary>
[Alias("Orders.InspectionCompleted.V1"), DomainEventType("Orders.InspectionCompleted.V1"), GenerateSerializer, Immutable]
public sealed class InspectionCompleted : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required DateOnly CompletedOn { get; init; }
}
