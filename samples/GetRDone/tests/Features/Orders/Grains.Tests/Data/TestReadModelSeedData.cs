using GetRDone.Orders.Grains.ReadModels;

namespace GetRDone.Orders.Grains.Tests.Data;

/// <summary>Seeds the <see cref="TestData"/> into the read models of the test tenant.</summary>
public sealed class TestReadModelSeedData : IReadModelSeedData
{
    private static readonly ReadModelSeed Seed = new(
        TestData.IntendedUses, TestData.DocumentTypes, TestData.OrderableServices, TestData.Users);

    public ReadModelSeed? GetSeed(string? tenantId) => tenantId == TestData.TenantId ? Seed : null;
}
