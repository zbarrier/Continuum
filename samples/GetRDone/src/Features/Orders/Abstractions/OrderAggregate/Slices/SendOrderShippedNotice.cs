using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Order Shipped Notifier told the client user who entered the order that its report is ready. Only
    ///     the Order Shipped Notifier automation may record it. Succeeds without changes when the notice was already sent, so
    ///     the automation can safely retry.
    /// </summary>
    Task<Result<SendOrderShippedNoticeResponse>> SendOrderShippedNoticeAsync(SendOrderShippedNoticeCommand command);

    /// <summary>
    ///     Whether the notice of the shipped order was sent, so the Order Shipped Notifier doesn't email it again when the
    ///     event that started it is delivered again. Only the Order Shipped Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsOrderShippedNoticeSentAsync(string documentId);
}

/// <summary>The notice of the shipped order the Order Shipped Notifier emailed: who it went to and its subject.</summary>
[Alias("Orders.SendOrderShippedNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendOrderShippedNoticeCommand(
    string DocumentId,
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendOrderShippedNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendOrderShippedNoticeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

/// <summary>The email telling the client user who entered the order that its report is ready went out.</summary>
[Alias("Orders.OrderShippedNoticeSent.V1"), DomainEventType("Orders.OrderShippedNoticeSent.V1"), GenerateSerializer, Immutable]
public sealed class OrderShippedNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DocumentId { get; init; }
    [Id(2)] public required string RecipientUserId { get; init; }
    [Id(3)] public required string RecipientEmail { get; init; }
    [Id(4)] public required string Subject { get; init; }
}
