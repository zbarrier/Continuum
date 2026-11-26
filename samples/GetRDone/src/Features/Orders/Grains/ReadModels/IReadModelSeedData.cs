using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>Supplies the reference data a tenant's read models are seeded with when they are first activated.</summary>
/// <remarks>The silo registers <see cref="SampleSeedData"/>; tests register their own data.</remarks>
public interface IReadModelSeedData
{
    /// <summary>Gets the data to seed into the tenant's read models, or <see langword="null"/> to leave them empty.</summary>
    ReadModelSeed? GetSeed(string? tenantId);
}

/// <summary>The reference data seeded into one tenant's read models.</summary>
public sealed record ReadModelSeed(
    IntendedUse[] IntendedUses,
    DocumentType[] DocumentTypes,
    OrderableService[] OrderableServices,
    UserDirectoryEntry[] Users,
    AssignableVendor[] Vendors);
