using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the New Order Notifier emailed the new order notice, with the order's number, to the AMC staff member
    ///     who assigns orders. Only the New Order Notifier automation may record it, once the order has its number. Succeeds
    ///     without changes when the notice was already sent, so the automation can safely retry.
    /// </summary>
    Task<Result<SendNewOrderNoticeResponse>> SendNewOrderNoticeAsync(SendNewOrderNoticeCommand command);

    /// <summary>
    ///     Whether the new order notice was sent, so the New Order Notifier doesn't email it again when the event that
    ///     started it is delivered again. Only the New Order Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsNewOrderNoticeSentAsync();
}

/// <summary>The notice the New Order Notifier emailed: who it went to and its subject.</summary>
[Alias("Orders.SendNewOrderNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendNewOrderNoticeCommand(
    string RecipientUserId,
    string RecipientEmail,
    string Subject);

[Alias("Orders.SendNewOrderNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendNewOrderNoticeResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The new order email, with the order's number, went out to the AMC staff member who assigns orders.</summary>
[Alias("Orders.NewOrderNoticeSent.V1"), DomainEventType("Orders.NewOrderNoticeSent.V1"), GenerateSerializer, Immutable]
public sealed class NewOrderNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required long OrderNumber { get; init; }
    [Id(2)] public required string RecipientUserId { get; init; }
    [Id(3)] public required string RecipientEmail { get; init; }
    [Id(4)] public required string Subject { get; init; }
}
