using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Assigns the order, once the client approved its description of work, to one of the tenant's assignable vendors,
    ///     with the vendor's fee and due date, and sets the order's service fee and due date for the client. The staff member the order is assigned to, the Client Services Director or the
    ///     tenant admin may assign it; the Vendor Assignment Notifier then tells the vendor's users.
    /// </summary>
    Task<Result<AssignVendorResponse>> AssignVendorAsync(AssignVendorCommand command);
}

/// <summary>
///     The vendor the order is assigned to, the fee the AMC pays it and the date its report is due; and the fee the client
///     pays the AMC and the date the client gets the report, usually a few days after the vendor's due date to leave time
///     for review and revisions.
/// </summary>
[Alias("Orders.AssignVendorCommand.V1"), GenerateSerializer, Immutable]
public sealed record AssignVendorCommand(
    string VendorId,
    decimal VendorFee,
    DateOnly VendorDueDate,
    decimal ServiceFee,
    DateOnly DueDate);

[Alias("Orders.AssignVendorResponse.V1"), GenerateSerializer, Immutable]
public sealed class AssignVendorResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>
///     The order was assigned to a vendor, with the approved description of work the vendor is to follow, and the order's
///     service fee and due date were set.
/// </summary>
/// <remarks>
///     It carries the order's number, when it has one, so the Vendor Assignment Notifier can name the order without
///     reading it. Unlike the event model's event, it doesn't copy the engagement details (client, property, form type,
///     access contact and the description of work's text): the vendor's read models take them from the earlier events.
///     Only the AMC's staff and the client see the service fee and due date; the vendor's read models leave them out.
/// </remarks>
[Alias("Orders.VendorAssigned.V1"), DomainEventType("Orders.VendorAssigned.V1"), GenerateSerializer, Immutable]
public sealed class VendorAssigned : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string VendorId { get; init; }
    [Id(2)] public required decimal VendorFee { get; init; }
    [Id(3)] public required DateOnly VendorDueDate { get; init; }
    [Id(4)] public required decimal ServiceFee { get; init; }
    [Id(5)] public required DateOnly DueDate { get; init; }
    [Id(6)] public required string DescriptionOfWorkId { get; init; }
    [Id(7)] public required int DescriptionOfWorkVersion { get; init; }
    [Id(8)] public long? OrderNumber { get; init; }
}
