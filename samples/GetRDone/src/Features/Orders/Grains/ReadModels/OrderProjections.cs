namespace GetRDone.Orders.Grains.ReadModels;

/// <summary>
///     Names shared by the order projections and the silo that hosts them.
/// </summary>
/// <remarks>
///     The silo reads the order streams with the KurrentDB stream provider named <see cref="ProviderName"/>, which
///     delivers each KurrentDB stream on the Orleans stream namespace <c>{ProviderName}:{Category}</c>. Order events
///     name the stream of the user who started the order as a stream target, so they also arrive on
///     <see cref="ClientUserOrdersNamespace"/>, keyed by that user. The client user's read models subscribe to that namespace
///     implicitly, so each one is a single grain per user that sees every order the user started.
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
}
