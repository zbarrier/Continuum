using Continuum.Orleans.Tenancy;

namespace GetRDone.Orders.Abstractions.ReadModels;

/// <summary>
///     Gets the read model grains. Each tenant has one instance of each reference read model, keyed <c>{tenantId}|default</c>,
///     so callers only pass the tenant ID. Per-user read models are keyed by the user's tenant user ID instead.
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

    /// <summary>Gets the drafts of one client user, keyed by their tenant user ID <c>{TenantId}|{UserGuid}</c>.</summary>
    public static IClientOrderDraftsGrain GetClientOrderDraftsGrain(this IGrainFactory grainFactory, string tenantUserId)
        => grainFactory.GetGrain<IClientOrderDraftsGrain>(tenantUserId);
}
