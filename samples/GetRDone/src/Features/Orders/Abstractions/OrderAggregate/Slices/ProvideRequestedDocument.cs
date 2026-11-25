using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    /// <summary>
    ///     Provides the document the AMC requested. Only the client user the document was requested from may provide
    ///     it, once per request. The document joins the order's supporting documents.
    /// </summary>
    Task<Result<ProvideRequestedDocumentResponse>> ProvideRequestedDocumentAsync(ProvideRequestedDocumentCommand command);
}

/// <summary>
///     The document the client provides for a request. Like <see cref="AttachSupportingDocumentCommand"/>, the client
///     generates the document ID and uploads the file to <see cref="SupportingDocumentStorage.BlobPath"/> first; the
///     document's type is the one requested.
/// </summary>
[Alias("Orders.ProvideRequestedDocumentCommand.V1"), GenerateSerializer, Immutable]
public sealed record ProvideRequestedDocumentCommand(
    string RequestId,
    string DocumentId,
    string FileName,
    string ContentType,
    long FileSize);

[Alias("Orders.ProvideRequestedDocumentResponse.V1"), GenerateSerializer, Immutable]
public sealed class ProvideRequestedDocumentResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>The client provided the document the AMC requested, with where its file is stored.</summary>
[Alias("Orders.RequestedDocumentProvided.V1"), DomainEventType("Orders.RequestedDocumentProvided.V1"), GenerateSerializer, Immutable]
public sealed class RequestedDocumentProvided : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string RequestId { get; init; }
    [Id(2)] public required string DocumentId { get; init; }
    [Id(3)] public required string DocumentTypeId { get; init; }
    [Id(4)] public required int DocumentTypeVersion { get; init; }
    [Id(5)] public required string DocumentTypeName { get; init; }
    [Id(6)] public required string FileName { get; init; }
    [Id(7)] public required string ContentType { get; init; }

    /// <summary>The size of the file in bytes.</summary>
    [Id(8)] public required long FileSize { get; init; }

    /// <summary>The path of the file within <see cref="SupportingDocumentStorage.ContainerName"/>.</summary>
    [Id(9)] public required string BlobPath { get; init; }
}
