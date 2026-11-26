using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Records that the Vendor Assignment Notifier told the users of the vendor the order is assigned to about it. Only
    ///     the Vendor Assignment Notifier automation may record it. Succeeds without changes when the notice was already
    ///     sent, so the automation can safely retry.
    /// </summary>
    Task<Result<SendVendorAssignedNoticeResponse>> SendVendorAssignedNoticeAsync(SendVendorAssignedNoticeCommand command);

    /// <summary>
    ///     Whether the notice of the assignment was sent, so the Vendor Assignment Notifier doesn't email it again when the
    ///     event that started it is delivered again. Only the Vendor Assignment Notifier automation may ask.
    /// </summary>
    [ReadOnly]
    Task<Result<bool>> IsVendorAssignedNoticeSentAsync(string vendorId);
}

/// <summary>The notice the Vendor Assignment Notifier emailed: the vendor's users it went to and its subject.</summary>
[Alias("Orders.SendVendorAssignedNoticeCommand.V1"), GenerateSerializer, Immutable]
public sealed record SendVendorAssignedNoticeCommand(
    string VendorId,
    VendorNoticeRecipient[] Recipients,
    string Subject);

/// <summary>A user of the vendor a notice was emailed to.</summary>
[Alias("Orders.VendorNoticeRecipient.V1"), GenerateSerializer, Immutable]
public sealed record VendorNoticeRecipient(
    string UserId,
    string Email);

[Alias("Orders.SendVendorAssignedNoticeResponse.V1"), GenerateSerializer, Immutable]
public sealed class SendVendorAssignedNoticeResponse(IReadOnlyList<IChange> changes, int version)
    : CommandResponseWithChangesBase(changes, version);

/// <summary>The email telling the vendor's users that the order was assigned to the vendor went out.</summary>
/// <remarks>The event model has a single recipient; a vendor can have several users, so this records each one emailed.</remarks>
[Alias("Orders.VendorAssignedNoticeSent.V1"), DomainEventType("Orders.VendorAssignedNoticeSent.V1"), GenerateSerializer, Immutable]
public sealed class VendorAssignedNoticeSent : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string VendorId { get; init; }
    [Id(2)] public required VendorNoticeRecipient[] Recipients { get; init; }
    [Id(3)] public required string Subject { get; init; }
}
