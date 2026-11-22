using Continuum.Orleans.Tenancy;

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
}

[GenerateSerializer]
public sealed class DocumentTypesState
{
    [Id(0)]
    public DocumentType[] DocumentTypes { get; set; } = [];
}
