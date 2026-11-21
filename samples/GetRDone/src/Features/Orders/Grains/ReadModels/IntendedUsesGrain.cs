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
        if (!state.RecordExists)
        {
            state.State.IntendedUses =
            [
                new IntendedUse(Guid.Parse("5c0f5b7e-6a1d-4f3e-9b2a-8d4c7e1f2a60"), 1, "Refinance"),
                new IntendedUse(Guid.Parse("a3e8d2c4-7b19-4f6a-8e05-1d9c3b7f4e28"), 1, "Purchase")
            ];
            await state.WriteStateAsync();
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<IntendedUse[]> GetIntendedUsesAsync() => Task.FromResult(state.State.IntendedUses.ToArray());
}

[GenerateSerializer]
public sealed class IntendedUsesState
{
    [Id(0)]
    public IntendedUse[] IntendedUses { get; set; } = [];
}
