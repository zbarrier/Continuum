using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Submits the order's description of work for the approval of the client who started the order, or a revision of it
    ///     while it waits for approval. The staff member the order is assigned to, or anyone who may assign orders, may
    ///     submit once the order is acknowledged; the Description of Work Approval Notifier then emails the client.
    /// </summary>
    Task<Result<SubmitDescriptionOfWorkResponse>> SubmitDescriptionOfWorkAsync(SubmitDescriptionOfWorkCommand command);
}

/// <summary>
///     The description of work to submit. The first submission is version 1 with an ID the staff member's client generates;
///     each revision keeps the ID and is one version more than the one it revises, so a revision based on an outdated
///     version is rejected.
/// </summary>
[Alias("Orders.SubmitDescriptionOfWorkCommand.V1"), GenerateSerializer, Immutable]
public sealed record SubmitDescriptionOfWorkCommand(
    string DescriptionOfWorkId,
    int DescriptionOfWorkVersion,
    string DescriptionOfWorkHtml);

[Alias("Orders.SubmitDescriptionOfWorkResponse.V1"), GenerateSerializer, Immutable]
public sealed class SubmitDescriptionOfWorkResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>A version of the order's description of work was submitted for the approval of the client who started the order.</summary>
/// <remarks>
///     It carries the order's number, when it has one, so the Description of Work Approval Notifier can name the order in
///     its email without reading the order.
/// </remarks>
[Alias("Orders.DescriptionOfWorkSubmitted.V1"), DomainEventType("Orders.DescriptionOfWorkSubmitted.V1"), GenerateSerializer, Immutable]
public sealed class DescriptionOfWorkSubmitted : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DescriptionOfWorkId { get; init; }
    [Id(2)] public required int DescriptionOfWorkVersion { get; init; }

    /// <summary>The sanitized HTML of the description of work.</summary>
    [Id(3)] public required string DescriptionOfWorkHtml { get; init; }

    /// <summary>The client user who started the order, who approves the description of work.</summary>
    [Id(4)] public required string ApproverUserId { get; init; }

    [Id(5)] public long? OrderNumber { get; init; }
}
