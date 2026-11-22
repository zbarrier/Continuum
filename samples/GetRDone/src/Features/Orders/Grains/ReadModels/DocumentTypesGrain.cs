using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("documentTypes")]
public sealed class DocumentTypesGrain(
    [PersistentState("documentTypes")] IPersistentState<DocumentTypesState> state,
    IReadModelSeedData seedData)
    : Grain, IDocumentTypesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && seedData.GetSeed(tenantId) is { } seed)
        {
            state.State.DocumentTypes = seed.DocumentTypes;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<DocumentType[]> GetDocumentTypesAsync() => Task.FromResult(state.State.DocumentTypes.ToArray());

    public Task<Result> IsCurrentDocumentTypeAsync(DocumentTypeRef documentType)
    {
        var isCurrent = state.State.DocumentTypes.Any(item =>
            item.DocumentTypeId == (string)documentType.Id
            && item.DocumentTypeVersion == (int)documentType.Version
            && item.Name == (string)documentType.Name);

        return Task.FromResult(isCurrent
            ? Result.Success()
            : Result.Failure(RequestErrors.NewInvalidArg("The selected document type is not available.")));
    }
}

[GenerateSerializer]
public sealed class DocumentTypesState
{
    [Id(0)]
    public DocumentType[] DocumentTypes { get; set; } = [];
}
