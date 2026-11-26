using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Description of Work Approved Notifier told the staff member the order is assigned to that the
    ///     client approved the description of work. Only the Description of Work Approved Notifier automation may record it.
    ///     Succeeds without changes when the notice was already sent, so the automation can safely retry.
    /// </summary>
    Task<Result<SendDescriptionOfWorkApprovedNoticeResponse>> SendDescriptionOfWorkApprovedNoticeAsync(
        SendDescriptionOfWorkApprovedNoticeCommand command);

    /// <summary>
    ///     Whether the notice of the approval was sent, so the Description of Work Approved Notifier doesn't email it again
    ///     when the event that started it is delivered again. Only the Description of Work Approved Notifier automation may
    ///     ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsDescriptionOfWorkApprovedNoticeSentAsync(string descriptionOfWorkId);
}

/// <summary>The approval notice the Description of Work Approved Notifier emailed: who it went to and its subject.</summary>
[Alias("Orders.SendDescriptionOfWorkApprovedNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendDescriptionOfWorkApprovedNoticeCommand(
    string DescriptionOfWorkId,
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendDescriptionOfWorkApprovedNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendDescriptionOfWorkApprovedNoticeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

/// <summary>The email telling the staff member the order is assigned to that the client approved the description of work went out.</summary>
[Alias("Orders.DescriptionOfWorkApprovedNoticeSent.V1"), DomainEventType("Orders.DescriptionOfWorkApprovedNoticeSent.V1"),
 GenerateSerializer, Immutable]
public sealed class DescriptionOfWorkApprovedNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DescriptionOfWorkId { get; init; }
    [Id(2)] public required string RecipientUserId { get; init; }
    [Id(3)] public required string RecipientEmail { get; init; }
    [Id(4)] public required string Subject { get; init; }
}
