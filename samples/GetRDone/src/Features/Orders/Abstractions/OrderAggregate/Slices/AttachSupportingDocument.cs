using Continuum.CSharpFunctionalExtensions;
using Continuum.Domain;
using Continuum.Domain.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.TypeMapping;

namespace GetRDone.Orders.Abstractions.OrderAggregate;

public partial interface IOrderGrain : IEventDrivenStateMachineGrain
{
    Task<Result<AttachSupportingDocumentResponse>> AttachSupportingDocumentAsync(AttachSupportingDocumentCommand command);
}

/// <summary>
///     Attaches a supporting document to a draft (order entry tab 3). The client generates the document ID, uploads the
///     file to <see cref="SupportingDocumentStorage.BlobPath"/> first, and then attaches it; the command carries only the
///     file's details, never its content. The description is optional.
/// </summary>
[Alias("Orders.AttachSupportingDocumentCommand.V1"), GenerateSerializer, Immutable]
public sealed record AttachSupportingDocumentCommand(
    string DocumentId,
    string DocumentTypeId,
    int DocumentTypeVersion,
    string DocumentTypeName,
    string FileName,
    string ContentType,
    long FileSize,
    string? Description);

[Alias("Orders.AttachSupportingDocumentResponse.V1"), GenerateSerializer, Immutable]
public sealed class AttachSupportingDocumentResponse(IReadOnlyList<IChange> changes, int version) : CommandResponseWithChangesBase(changes, version);

/// <summary>A supporting document attached to an order, with where its file is stored.</summary>
[Alias("Orders.SupportingDocumentAttached.V1"), DomainEventType("Orders.SupportingDocumentAttached.V1"), GenerateSerializer, Immutable]
public sealed class SupportingDocumentAttached : OrderEvent
{
    [Id(0)] public required string OrderId { get; init; }
    [Id(1)] public required string DocumentId { get; init; }
    [Id(2)] public required string DocumentTypeId { get; init; }
    [Id(3)] public required int DocumentTypeVersion { get; init; }
    [Id(4)] public required string DocumentTypeName { get; init; }
    [Id(5)] public required string FileName { get; init; }
    [Id(6)] public required string ContentType { get; init; }

    /// <summary>The size of the file in bytes.</summary>
    [Id(7)] public required long FileSize { get; init; }

    [Id(8)] public string? Description { get; init; }

    /// <summary>The path of the file within <see cref="SupportingDocumentStorage.ContainerName"/>.</summary>
    [Id(9)] public required string BlobPath { get; init; }
}

/// <summary>
///     Where the files of supporting documents are stored. The client uploads a file to its path before attaching the
///     document, and the order records the same path, so both derive it from the order and document IDs here.
/// </summary>
public static class SupportingDocumentStorage
{
    /// <summary>The blob container that holds the files of every tenant's supporting documents.</summary>
    public const string ContainerName = "order-documents";

    /// <summary>
    ///     The path of a document's file within <see cref="ContainerName"/>: <c>{tenantId}/{orderGuid}/{documentGuid}</c>,
    ///     so each tenant's files share a prefix.
    /// </summary>
    public static string BlobPath(string orderId, string documentId)
    {
        TenantKey.Split(orderId, out var tenantId, out var orderKey);
        TenantKey.Split(documentId, out _, out var documentKey);
        return $"{tenantId}/{orderKey}/{documentKey}";
    }
}
