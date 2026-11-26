using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Description of Work Approval Notifier emailed the client a version of the description of work to
    ///     approve. Only the Description of Work Approval Notifier automation may record it. Succeeds without changes when the
    ///     notice of that version, or of a later one, was already recorded, so the automation can safely retry.
    /// </summary>
    Task<Result<SendDescriptionOfWorkSubmittedNoticeResponse>> SendDescriptionOfWorkSubmittedNoticeAsync(
        SendDescriptionOfWorkSubmittedNoticeCommand command);

    /// <summary>
    ///     Whether the client should be emailed a version of the description of work: it is the version that waits for
    ///     approval and its notice wasn't sent. The Description of Work Approval Notifier asks, so it doesn't email a version
    ///     again when the event that started it is delivered again, nor email a version that was revised or approved since.
    ///     Only the Description of Work Approval Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsDescriptionOfWorkSubmittedNoticeDueAsync(string descriptionOfWorkId, int descriptionOfWorkVersion);
}

/// <summary>The approval request the Description of Work Approval Notifier emailed: the version, who it went to and its subject.</summary>
[Alias("Orders.SendDescriptionOfWorkSubmittedNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendDescriptionOfWorkSubmittedNoticeCommand(
    string DescriptionOfWorkId,
    int DescriptionOfWorkVersion,
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendDescriptionOfWorkSubmittedNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendDescriptionOfWorkSubmittedNoticeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

/// <summary>The email asking the client to approve a version of the description of work went out.</summary>
[Alias("Orders.DescriptionOfWorkSubmittedNoticeSent.V1"), DomainEventType("Orders.DescriptionOfWorkSubmittedNoticeSent.V1"),
 GenerateSerializer, Immutable]
public sealed class DescriptionOfWorkSubmittedNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DescriptionOfWorkId { get; init; }
    [Id(2)] public required int DescriptionOfWorkVersion { get; init; }
    [Id(3)] public required string RecipientUserId { get; init; }
    [Id(4)] public required string RecipientEmail { get; init; }
    [Id(5)] public required string Subject { get; init; }
}
