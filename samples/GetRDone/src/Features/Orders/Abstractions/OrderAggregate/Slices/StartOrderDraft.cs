using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenGrain
{
    Task<Result<StartOrderDraftResponse>> StartOrderDraftAsync(StartOrderDraftCommand command);
}

[GenerateSerializer, Immutable]
public sealed record StartOrderDraftCommand(
    Guid ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    Guid FormTypeId,
    int FormTypeVersion,
    string PropertyStreet,
    string PropertyCity,
    string PropertyState,
    string PropertyZip,
    string PropertyType,
    string IntendedUseId,
    string IntendedUseVersion,
    string IntendedUseName,
    string LoanNumber,
    DateOnly DueDate);

[GenerateSerializer, Immutable]
public sealed class StartOrderDraftResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

[DomainEventType("GetRDone.Orders.OrderDraftStarted"), GenerateSerializer, Immutable]
public sealed class OrderDraftStarted : OrderEvent
{
    [Id(0)] public required Guid OrderId { get; init; }
    [Id(1)] public required Guid ServiceRequestTypeId { get; init; }
    [Id(2)] public required int ServiceRequestTypeVersion { get; init; }
    [Id(3)] public required string ServiceRequestTypeName { get; init; }
    [Id(4)] public required Guid FormTypeId { get; init; }
    [Id(5)] public required int FormTypeVersion { get; init; }
    [Id(6)] public required string FormTypeName { get; init; }
    [Id(7)] public required string PropertyStreet { get; init; }
    [Id(8)] public required string PropertyCity { get; init; }
    [Id(9)] public required string PropertyState { get; init; }
    [Id(10)] public required string PropertyZip { get; init; }
    [Id(11)] public required string PropertyType { get; init; }
    [Id(12)] public required string IntendedUseId { get; init; }
    [Id(13)] public required string IntendedUseVersion { get; init; }
    [Id(14)] public required string IntendedUseName { get; init; }
    [Id(15)] public required string LoanNumber { get; init; }
    [Id(16)] public required DateOnly DueDate { get; init; }
}
