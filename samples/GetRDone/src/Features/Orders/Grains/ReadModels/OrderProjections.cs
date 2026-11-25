using Continuum.Orleans.Tenancy;

using GetRDone.Orders.Abstractions.ReadModels;

namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     Names shared by the order projections and the silo that hosts them.
/// </summary>
/// <remarks>
///     The silo reads the order streams with the KurrentDB stream provider named <see cref="ProviderName"/>, which
///     delivers each KurrentDB stream on the Orleans stream namespace <c>{ProviderName}:{Category}</c>, so each order's own
///     events arrive on <see cref="OrderEventsNamespace"/>, keyed by the order ID. Order events also name two stream targets,
///     so they also arrive on <see cref="ClientUserOrdersNamespace"/>, keyed by the user who started the order, and on
///     <see cref="TenantOrdersNamespace"/>, keyed by the tenant. Read models subscribe to one of these namespaces
///     implicitly, so each one is a single grain per order, per client user or per tenant that sees every event it needs.
/// </remarks>
public static class OrderProjections
{
    /// <summary>The name of the stream provider that reads the order streams.</summary>
    public const string ProviderName = "OrderProjections";

    /// <summary>The grain storage that projection state is written to.</summary>
    public const string StorageName = "Projections";

    /// <summary>
    ///     The category of the per-user stream, <c>clientUserOrders-{TenantId}|{UserGuid}</c>, that groups the orders a
    ///     client user started. Its key is the user's tenant user ID.
    /// </summary>
    public const string ClientUserOrdersCategory = "clientUserOrders";

    /// <summary>The Orleans stream namespace the provider delivers the per-user client orders stream on.</summary>
    public const string ClientUserOrdersNamespace = ProviderName + ":" + ClientUserOrdersCategory;

    /// <summary>The stream target that sends an order's events to the client user orders stream of <paramref name="tenantUserId"/>.</summary>
    public static GrainId ClientUserOrdersStreamTarget(string tenantUserId)
        => GrainId.Create(ClientUserOrdersCategory, tenantUserId);

    /// <summary>The Orleans stream namespace the provider delivers each order's own stream, <c>order-{TenantId}|{OrderGuid}</c>, on.</summary>
    public const string OrderEventsNamespace = ProviderName + ":" + OrderReactors.OrderCategory;

    /// <summary>
    ///     The category of the per-tenant stream, <c>tenantOrders-{TenantId}|default</c>, that groups every order of a tenant
    ///     for the staff dashboard. Its key is the tenant's read model key, as the dashboard grains are keyed.
    /// </summary>
    public const string TenantOrdersCategory = "tenantOrders";

    /// <summary>The Orleans stream namespace the provider delivers the per-tenant orders stream on.</summary>
    public const string TenantOrdersNamespace = ProviderName + ":" + TenantOrdersCategory;

    /// <summary>The stream target that sends an order's events to the orders stream of <paramref name="tenantId"/>.</summary>
    public static GrainId TenantOrdersStreamTarget(string tenantId)
        => GrainId.Create(TenantOrdersCategory, TenantKey.Create(tenantId, ReadModelGrainFactoryExtensions.DefaultKey));
}
