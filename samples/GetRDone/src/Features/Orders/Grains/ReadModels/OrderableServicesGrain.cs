using GetRDone.Orders.Abstractions.ReadModels;

using Microsoft.AspNetCore.Authorization;

namespace GetRDone.Orders.Grains.ReadModels;

[Authorize]
[GrainType("orderableServices")]
public sealed class OrderableServicesGrain(
    [PersistentState("offerings")] IPersistentState<OrderableServicesState> state)
    : Grain, IOrderableServicesGrain
{
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!state.RecordExists)
        {
            state.State.Offerings =
            [
                new OrderableService(
                    Guid.Parse("d487ac18-f4ae-402c-828e-2b9e8d09516c"), 1, "Appraisal Commercial",
                    Guid.Parse("78a59b0d-e8ea-4f85-9860-c206ab297170"), 1, "Market Value Interior Inspection")
            ];
            await state.WriteStateAsync();
        }
        await base.OnActivateAsync(cancellationToken);
    }

    public Task<OrderableService[]> GetOfferingsAsync() => Task.FromResult(state.State.Offerings.ToArray());
}

[GenerateSerializer]
public sealed class OrderableServicesState
{
    [Id(0)]
    public OrderableService[] Offerings { get; set; } = [];
}
