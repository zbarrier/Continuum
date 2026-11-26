using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Approves the version of the description of work that waits for approval. Only the client user who started the
    ///     order may approve it; the Description of Work Approved Notifier then tells the staff member the order is
    ///     assigned to.
    /// </summary>
    Task<Result<ApproveDescriptionOfWorkResponse>> ApproveDescriptionOfWorkAsync(ApproveDescriptionOfWorkCommand command);
}

/// <summary>
///     The description of work the client approves, with the version they reviewed, so a version revised after they
///     opened it isn't approved unseen.
/// </summary>
[Alias("Orders.ApproveDescriptionOfWorkCommand.V1"), GenerateSerializer, Immutable]
public sealed record ApproveDescriptionOfWorkCommand(
    string DescriptionOfWorkId,
    int DescriptionOfWorkVersion);

[Alias("Orders.ApproveDescriptionOfWorkResponse.V1"), GenerateSerializer, Immutable]
public sealed class ApproveDescriptionOfWorkResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The client who started the order approved a version of its description of work.</summary>
/// <remarks>
///     It carries the order's assignee and number, when it has one, so the Description of Work Approved Notifier can email
///     the assignee and name the order without reading the order.
/// </remarks>
[Alias("Orders.DescriptionOfWorkApproved.V1"), DomainEventType("Orders.DescriptionOfWorkApproved.V1"), GenerateSerializer, Immutable]
public sealed class DescriptionOfWorkApproved : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DescriptionOfWorkId { get; init; }
    [Id(2)] public required int DescriptionOfWorkVersion { get; init; }

    /// <summary>The staff member the order is assigned to, who is told about the approval.</summary>
    [Id(3)] public required string AssignedToUserId { get; init; }

    [Id(4)] public long? OrderNumber { get; init; }
}
