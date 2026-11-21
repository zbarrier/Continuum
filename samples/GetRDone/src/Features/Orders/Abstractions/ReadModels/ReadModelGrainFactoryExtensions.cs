using Continuum.Orleans.Tenancy;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     Gets the read model grains. Each tenant has one instance of each, keyed <c>{tenantId}|default</c>, so callers only
///     pass the tenant ID.
/// </summary>
public static class ReadModelGrainFactoryExtensions
{
    /// <summary>The key within the tenant of the single read model grain per tenant (a tenant key needs a non-empty key).</summary>
    public const string DefaultKey = "default";

    public static IIntendedUsesGrain GetIntendedUsesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IIntendedUsesGrain>(TenantKey.Create(tenantId, DefaultKey));

    public static IOrderableServicesGrain GetOrderableServicesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create(tenantId, DefaultKey));

    public static IUserDirectoryGrain GetUserDirectoryGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IUserDirectoryGrain>(TenantKey.Create(tenantId, DefaultKey));
}
