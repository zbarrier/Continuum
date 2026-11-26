using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Delivers the vendor's completed report, once the inspection is completed. Only users of the vendor the order is
    ///     assigned to may deliver it, once; the Report Delivered Notifier then tells the staff member the order is assigned
    ///     to.
    /// </summary>
    Task<Result<DeliverReportResponse>> DeliverReportAsync(DeliverReportCommand command);
}

/// <summary>
///     The vendor's completed report. Like <see cref="AttachSupportingDocumentCommand"/>, the vendor generates the document
///     ID and uploads the file to <see cref="SupportingDocumentStorage.BlobPath"/> first, so the report is stored with the
///     order's other documents; the command carries only the file's details.
/// </summary>
[Alias("Orders.DeliverReportCommand.V1"), GenerateSerializer, Immutable]
public sealed record DeliverReportCommand(
    string DocumentId,
    string FileName,
    string ContentType,
    long FileSize);

[Alias("Orders.DeliverReportResponse.V1"), GenerateSerializer, Immutable]
public sealed class DeliverReportResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The vendor delivered its completed report, with where its file is stored.</summary>
/// <remarks>
///     It carries the staff member the order is assigned to and the order's number, when it has one, so the Report
///     Delivered Notifier can tell them without reading the order. The client doesn't see the report until the AMC ships
///     it.
/// </remarks>
[Alias("Orders.ReportDelivered.V1"), DomainEventType("Orders.ReportDelivered.V1"), GenerateSerializer, Immutable]
public sealed class ReportDelivered : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DocumentId { get; init; }
    [Id(2)] public required string FileName { get; init; }
    [Id(3)] public required string ContentType { get; init; }

    /// <summary>The size of the file in bytes.</summary>
    [Id(4)] public required long FileSize { get; init; }

    /// <summary>The path of the file within <see cref="SupportingDocumentStorage.ContainerName"/>.</summary>
    [Id(5)] public required string BlobPath { get; init; }

    /// <summary>The staff member the order is assigned to, who is told about the report.</summary>
    [Id(6)] public required string AssignedToUserId { get; init; }

    [Id(7)] public long? OrderNumber { get; init; }
}
