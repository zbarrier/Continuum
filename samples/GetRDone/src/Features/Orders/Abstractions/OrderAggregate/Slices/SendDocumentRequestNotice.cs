using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Document Request Notifier emailed the client the document request. Only the Document Request
    ///     Notifier automation may record it. Succeeds without changes when the notice was already sent, so the automation
    ///     can safely retry.
    /// </summary>
    Task<Result<SendDocumentRequestNoticeResponse>> SendDocumentRequestNoticeAsync(SendDocumentRequestNoticeCommand command);

    /// <summary>
    ///     Whether the notice of a document request was sent, so the Document Request Notifier doesn't email it again when
    ///     the event that started it is delivered again. Only the Document Request Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsDocumentRequestNoticeSentAsync(string requestId);
}

/// <summary>The document request notice the Document Request Notifier emailed: who it went to and its subject.</summary>
[Alias("Orders.SendDocumentRequestNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendDocumentRequestNoticeCommand(
    string RequestId,
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendDocumentRequestNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendDocumentRequestNoticeResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The email asking the client for the requested documents went out.</summary>
[Alias("Orders.DocumentRequestNoticeSent.V1"), DomainEventType("Orders.DocumentRequestNoticeSent.V1"), GenerateSerializer, Immutable]
public sealed class DocumentRequestNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string RequestId { get; init; }
    [Id(2)] public required string RecipientUserId { get; init; }
    [Id(3)] public required string RecipientEmail { get; init; }
    [Id(4)] public required string Subject { get; init; }
}
