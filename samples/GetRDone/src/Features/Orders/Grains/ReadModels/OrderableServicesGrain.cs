using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("orderableServices")]
public sealed class OrderableServicesGrain(
    [PersistentState("orderableServices")] IPersistentState<OrderableServicesState> state)
    : Grain, IOrderableServicesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && tenantId == SeedData.TenantId)
        {
            state.State.OrderableServices = SeedData.OrderableServices;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<OrderableService[]> GetOrderableServicesAsync() => Task.FromResult(state.State.OrderableServices.ToArray());

    public Task<Result> IsCurrentOrderableServiceAsync(
        ServiceRequestTypeId serviceRequestTypeId,
        ServiceRequestTypeVersion serviceRequestTypeVersion,
        ServiceRequestTypeName serviceRequestTypeName,
        FormTypeId formTypeId,
        FormTypeVersion formTypeVersion,
        FormTypeName formTypeName)
    {
        var isCurrent = state.State.OrderableServices.Any(item =>
            item.ServiceRequestTypeId == (string)serviceRequestTypeId
            && item.ServiceRequestTypeVersion == (int)serviceRequestTypeVersion
            && item.ServiceRequestTypeName == (string)serviceRequestTypeName
            && item.FormTypeId == (string)formTypeId
            && item.FormTypeVersion == (int)formTypeVersion
            && item.FormTypeName == (string)formTypeName);

        return Task.FromResult(isCurrent
            ? Result.Success()
            : Result.Failure(RequestErrors.NewInvalidArg("The selected service and form are not available.")));
    }
}

[GenerateSerializer]
public sealed class OrderableServicesState
{
    [Id(0)]
    public OrderableService[] OrderableServices { get; set; } = [];
}
