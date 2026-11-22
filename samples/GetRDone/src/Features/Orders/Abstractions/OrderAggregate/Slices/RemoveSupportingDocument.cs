using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    Task<Result<RemoveSupportingDocumentResponse>> RemoveSupportingDocumentAsync(RemoveSupportingDocumentCommand command);
}

/// <summary>
///     Removes a supporting document from a draft (order entry tab 3). The event records where the document's file is
///     stored, so whoever removed it can delete the file.
/// </summary>
[Alias("Orders.RemoveSupportingDocumentCommand.V1"), GenerateSerializer, Immutable]
public sealed record RemoveSupportingDocumentCommand(string DocumentId);

[Alias("Orders.RemoveSupportingDocumentResponse.V1"), GenerateSerializer, Immutable]
public sealed class RemoveSupportingDocumentResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>A supporting document removed from an order, with where its file was stored.</summary>
[Alias("Orders.SupportingDocumentRemoved.V1"), DomainEventType("Orders.SupportingDocumentRemoved.V1"), GenerateSerializer, Immutable]
public sealed class SupportingDocumentRemoved : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DocumentId { get; init; }

    /// <summary>The path of the document's file within <see cref="SupportingDocumentStorage.ContainerName"/>, as it was attached.</summary>
    [Id(2)] public required string BlobPath { get; init; }
}
