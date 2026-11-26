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

    public static IDocumentTypesGrain GetDocumentTypesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IDocumentTypesGrain>(TenantKey.Create(tenantId, DefaultKey));

    public static IOrderableServicesGrain GetOrderableServicesGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IOrderableServicesGrain>(TenantKey.Create(tenantId, DefaultKey));

    public static IUserDirectoryGrain GetUserDirectoryGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IUserDirectoryGrain>(TenantKey.Create(tenantId, DefaultKey));

    public static IAssignableVendorsGrain GetAssignableVendorsGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IAssignableVendorsGrain>(TenantKey.Create(tenantId, DefaultKey));

    /// <summary>Gets the drafts of one client user, keyed by their tenant user ID <c>{TenantId}|{UserGuid}</c>.</summary>
    public static IClientOrderDraftsGrain GetClientOrderDraftsGrain(this IGrainFactory grainFactory, string tenantUserId)
        => grainFactory.GetGrain<IClientOrderDraftsGrain>(tenantUserId);

    /// <summary>Gets the submitted orders of one client user, keyed by their tenant user ID <c>{TenantId}|{UserGuid}</c>.</summary>
    public static IClientOrdersGrain GetClientOrdersGrain(this IGrainFactory grainFactory, string tenantUserId)
        => grainFactory.GetGrain<IClientOrdersGrain>(tenantUserId);

    /// <summary>Gets the staff dashboard's Unassigned list of the tenant.</summary>
    public static IUnassignedOrdersGrain GetUnassignedOrdersGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IUnassignedOrdersGrain>(TenantKey.Create(tenantId, DefaultKey));

    /// <summary>Gets the staff dashboard's In progress list of the tenant.</summary>
    public static IInProgressOrdersGrain GetInProgressOrdersGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IInProgressOrdersGrain>(TenantKey.Create(tenantId, DefaultKey));

    /// <summary>Gets the staff order page of one order, keyed by the order ID <c>{TenantId}|{OrderGuid}</c>.</summary>
    public static IOrderDetailsGrain GetOrderDetailsGrain(this IGrainFactory grainFactory, string orderId)
        => grainFactory.GetGrain<IOrderDetailsGrain>(orderId);

    /// <summary>Gets the client order page of one order, keyed by the order ID <c>{TenantId}|{OrderGuid}</c>.</summary>
    public static IClientOrderDetailsGrain GetClientOrderDetailsGrain(this IGrainFactory grainFactory, string orderId)
        => grainFactory.GetGrain<IClientOrderDetailsGrain>(orderId);

    /// <summary>Gets the orders the tenant assigned to vendors; each vendor user sees only their own vendor's.</summary>
    public static IVendorOrdersGrain GetVendorOrdersGrain(this IGrainFactory grainFactory, string? tenantId)
        => grainFactory.GetGrain<IVendorOrdersGrain>(TenantKey.Create(tenantId, DefaultKey));

    /// <summary>Gets the vendor order page of one order, keyed by the order ID <c>{TenantId}|{OrderGuid}</c>.</summary>
    public static IVendorOrderDetailsGrain GetVendorOrderDetailsGrain(this IGrainFactory grainFactory, string orderId)
        => grainFactory.GetGrain<IVendorOrderDetailsGrain>(orderId);
}
