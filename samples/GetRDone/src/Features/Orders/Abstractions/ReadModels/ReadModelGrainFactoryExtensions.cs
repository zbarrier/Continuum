using Continuum.Orleans.Tenancy;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>Gets the per-tenant read model grains, so callers don't repeat their keys.</summary>
public static class ReadModelGrainFactoryExtensions
{
    public static IIntendedUsesGrain GetIntendedUsesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IIntendedUsesGrain>(TenantKey.Create(tenantId, "intendedUses"));

    public static IOrderableServicesGrain GetOrderableServicesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create(tenantId, "orderableServices"));

    public static IUserDirectoryGrain GetUserDirectoryGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IUserDirectoryGrain>(TenantKey.Create(tenantId, "userDirectory"));
}
