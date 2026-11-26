using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Schedules the property's inspection, once the order is assigned to a vendor. Only users of that vendor may
    ///     schedule it, once.
    /// </summary>
    Task<Result<ScheduleInspectionResponse>> ScheduleInspectionAsync(ScheduleInspectionCommand command);
}

/// <summary>When the appraiser inspects the property and who meets them there, usually the order's access contact.</summary>
[Alias("Orders.ScheduleInspectionCommand.V1"), GenerateSerializer, Immutable]
public sealed record ScheduleInspectionCommand(
    DateTimeOffset InspectionAt,
    string ContactName);

[Alias("Orders.ScheduleInspectionResponse.V1"), GenerateSerializer, Immutable]
public sealed class ScheduleInspectionResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The vendor scheduled the property's inspection.</summary>
[Alias("Orders.InspectionScheduled.V1"), DomainEventType("Orders.InspectionScheduled.V1"), GenerateSerializer, Immutable]
public sealed class InspectionScheduled : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required DateTimeOffset InspectionAt { get; init; }
    [Id(2)] public required string ContactName { get; init; }
}
