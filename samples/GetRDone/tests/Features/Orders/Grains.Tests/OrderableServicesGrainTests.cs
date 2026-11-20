using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Tests;

public sealed class OrderableServicesGrainTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    [Fact]
    public async Task Offerings_AreSeededForEachTenantAndRemainStableAcrossCalls()
    {
        var first = fixture.Cluster.GrainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create("tenant-a", "offerings"));
        var second = fixture.Cluster.GrainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create("tenant-b", "offerings"));

        var firstOffering = Assert.Single(await first.GetOfferingsAsync());
        var secondOffering = Assert.Single(await second.GetOfferingsAsync());
        Assert.Equal(new OrderableService(
            Guid.Parse("d487ac18-f4ae-402c-828e-2b9e8d09516c"), 1, "Appraisal Commercial",
            Guid.Parse("78a59b0d-e8ea-4f85-9860-c206ab297170"), 1, "Market Value Interior Inspection"), firstOffering);
        Assert.Equal(firstOffering, secondOffering);
        Assert.Equal(firstOffering, Assert.Single(await first.GetOfferingsAsync()));
        Assert.NotEqual(first.GetGrainId(), second.GetGrainId());
    }
}
