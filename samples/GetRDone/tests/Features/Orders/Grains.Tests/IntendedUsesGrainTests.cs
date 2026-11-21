using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Tests;

public sealed class IntendedUsesGrainTests(OrdersCluster fixture) : IClassFixture<OrdersCluster>
{
    [Fact]
    public async Task IntendedUses_AreSeededForEachTenantAndRemainStableAcrossCalls()
    {
        var first = fixture.Cluster.GrainFactory.GetGrain<IIntendedUsesGrain>(TenantKey.Create("tenant-a", "intendedUses"));
        var second = fixture.Cluster.GrainFactory.GetGrain<IIntendedUsesGrain>(TenantKey.Create("tenant-b", "intendedUses"));

        var firstIntendedUses = await first.GetIntendedUsesAsync();
        Assert.Equal(
        [
            new IntendedUse(Guid.Parse("5c0f5b7e-6a1d-4f3e-9b2a-8d4c7e1f2a60"), 1, "Refinance"),
            new IntendedUse(Guid.Parse("a3e8d2c4-7b19-4f6a-8e05-1d9c3b7f4e28"), 1, "Purchase")
        ], firstIntendedUses);
        Assert.Equal(firstIntendedUses, await second.GetIntendedUsesAsync());
        Assert.Equal(firstIntendedUses, await first.GetIntendedUsesAsync());
        Assert.NotEqual(first.GetGrainId(), second.GetGrainId());
    }
}
