using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    Task<Result<SaveRequestAndPropertyResponse>> SaveRequestAndPropertyAsync(SaveRequestAndPropertyCommand command);
}

/// <summary>
///     Saves the Request &amp; Property tab of a draft that was already started (<see cref="StartOrderDraftCommand"/> saves it
///     the first time). It has the same fields and rules, except the client, which the draft keeps.
/// </summary>
[Alias("Orders.SaveRequestAndPropertyCommand.V1"), GenerateSerializer, Immutable]
public sealed record SaveRequestAndPropertyCommand(
    string ServiceRequestTypeId,
    int ServiceRequestTypeVersion,
    string ServiceRequestTypeName,
    string FormTypeId,
    int FormTypeVersion,
    string FormTypeName,
    string PropertyStreet,
    string PropertyCity,
    string PropertyState,
    string PropertyZip,
    string PropertyType,
    string IntendedUseId,
    int IntendedUseVersion,
    string IntendedUseName,
    string LoanNumber,
    DateOnly DueDate);

[Alias("Orders.SaveRequestAndPropertyResponse.V1"), GenerateSerializer, Immutable]
public sealed class SaveRequestAndPropertyResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The Request &amp; Property tab saved again; it replaces everything <see cref="OrderDraftStarted"/> recorded except the client.</summary>
[Alias("Orders.RequestAndPropertySaved.V1"), DomainEventType("Orders.RequestAndPropertySaved.V1"), GenerateSerializer, Immutable]
public sealed class RequestAndPropertySaved : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string ServiceRequestTypeId { get; init; }
    [Id(2)] public required int ServiceRequestTypeVersion { get; init; }
    [Id(3)] public required string ServiceRequestTypeName { get; init; }
    [Id(4)] public required string FormTypeId { get; init; }
    [Id(5)] public required int FormTypeVersion { get; init; }
    [Id(6)] public required string FormTypeName { get; init; }
    [Id(7)] public required string PropertyStreet { get; init; }
    [Id(8)] public required string PropertyCity { get; init; }
    [Id(9)] public required string PropertyState { get; init; }
    [Id(10)] public required string PropertyZip { get; init; }
    [Id(11)] public required string PropertyType { get; init; }
    [Id(12)] public required string IntendedUseId { get; init; }
    [Id(13)] public required int IntendedUseVersion { get; init; }
    [Id(14)] public required string IntendedUseName { get; init; }
    [Id(15)] public required string LoanNumber { get; init; }
    [Id(16)] public required DateOnly DueDate { get; init; }
}
