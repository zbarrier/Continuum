using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("intendedUses")]
public sealed class IntendedUsesGrain(
    [PersistentState("intendedUses")] IPersistentState<IntendedUsesState> state,
    IReadModelSeedData seedData)
    : Grain, IIntendedUsesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && seedData.GetSeed(tenantId) is { } seed)
        {
            state.State.IntendedUses = seed.IntendedUses;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<IntendedUse[]> GetIntendedUsesAsync() => Task.FromResult(state.State.IntendedUses.ToArray());

    public Task<Result> IsCurrentIntendedUseAsync(IntendedUseRef intendedUse)
    {
        var isCurrent = state.State.IntendedUses.Any(item =>
            item.IntendedUseId == (string)intendedUse.Id
            && item.IntendedUseVersion == (int)intendedUse.Version
            && item.Name == (string)intendedUse.Name);

        return Task.FromResult(isCurrent
            ? Result.Success()
            : Result.Failure(RequestErrors.NewInvalidArg("The selected intended use is not available.")));
    }
}

[GenerateSerializer]
public sealed class IntendedUsesState
{
    [Id(0)]
    public IntendedUse[] IntendedUses { get; set; } = [];
}
