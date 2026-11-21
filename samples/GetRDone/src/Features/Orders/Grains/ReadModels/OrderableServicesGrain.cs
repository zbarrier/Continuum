using Continuum.CSharpFunctionalExtensions;
using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions;
using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("orderableServices")]
public sealed class OrderableServicesGrain(
    [PersistentState("orderableServices")] IPersistentState<OrderableServicesState> state,
    IReadModelSeedData seedData)
    : Grain, IOrderableServicesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantKey.Split(this.GetPrimaryKeyString(), out var tenantId, out _);
        if (!state.RecordExists && seedData.GetSeed(tenantId) is { } seed)
        {
            state.State.OrderableServices = seed.OrderableServices;
            await state.WriteStateAsync(cancellationToken);
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<OrderableService[]> GetOrderableServicesAsync() => Task.FromResult(state.State.OrderableServices.ToArray());

    public Task<Result> IsCurrentOrderableServiceAsync(ServiceRequestTypeRef serviceRequestType, FormTypeRef formType)
    {
        var isCurrent = state.State.OrderableServices.Any(service =>
            service.ServiceRequestTypeId == (string)serviceRequestType.Id
            && service.ServiceRequestTypeVersion == (int)serviceRequestType.Version
            && service.ServiceRequestTypeName == (string)serviceRequestType.Name
            && service.FormTypes.Any(form =>
                form.FormTypeId == (string)formType.Id
                && form.FormTypeVersion == (int)formType.Version
                && form.FormTypeName == (string)formType.Name));

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
