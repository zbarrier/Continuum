using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("assignableVendors")]
public sealed class AssignableVendorsGrain(
    [PersistentState("assignableVendors")] IPersistentState<AssignableVendorsState> state,
    IReadModelSeedData seedData)
    : Grain, IAssignableVendorsGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && seedData.GetSeed(tenantId) is { } seed)
        {
            state.State.Vendors = seed.Vendors;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<AssignableVendor[]> GetVendorsAsync() => Task.FromResult(state.State.Vendors.ToArray());

    public Task<Result> IsAssignableVendorAsync(VendorId vendorId)
    {
        var isAssignable = state.State.Vendors.Any(item => item.VendorId == (string)vendorId);

        return Task.FromResult(isAssignable
            ? Result.Success()
            : Result.Failure(RequestErrors.NewNotFound("The vendor is not one the AMC can assign orders to.")));
    }
}

[GenerateSerializer]
public sealed class AssignableVendorsState
{
    [Id(0)]
    public AssignableVendor[] Vendors { get; set; } = [];
}
