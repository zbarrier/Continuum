using Orleans.Concurrency;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     The types a supporting document can have (order entry tab 3, document requests). The sample seeds them per tenant
///     instead of projecting them from document type events.
/// </summary>
public interface IDocumentTypesGrain : IGrainWithStringKey
{
    [ReadOnly]
    Task<DocumentType[]> GetDocumentTypesAsync();
}

[GenerateSerializer, Immutable]
public sealed record DocumentType(
    string DocumentTypeId,
    int DocumentTypeVersion,
    string Name);
