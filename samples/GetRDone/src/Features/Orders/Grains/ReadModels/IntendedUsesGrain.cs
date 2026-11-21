using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("intendedUses")]
public sealed class IntendedUsesGrain(
    [PersistentState("intendedUses")] IPersistentState<IntendedUsesState> state)
    : Grain, IIntendedUsesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && tenantId == SeedData.TenantId)
        {
            state.State.IntendedUses = SeedData.IntendedUses;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<IntendedUse[]> GetIntendedUsesAsync() => Task.FromResult(state.State.IntendedUses.ToArray());

    public Task<Result> IsCurrentIntendedUseAsync(IntendedUseId intendedUseId, IntendedUseVersion intendedUseVersion, IntendedUseName intendedUseName)
    {
        var isCurrent = state.State.IntendedUses.Any(item =>
            item.IntendedUseId == (string)intendedUseId
            && item.IntendedUseVersion == (int)intendedUseVersion
            && item.Name == (string)intendedUseName);

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
