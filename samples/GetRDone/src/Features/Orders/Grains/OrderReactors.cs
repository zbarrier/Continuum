namespace GetRDone.Orders.Grains;

/// <summary>
///     Names shared by the order reactions and the silo that hosts them.
/// </summary>
/// <remarks>
///     <para>
///         A reaction runs a command when an event happens, such as giving an order the number the generator issued it.
///         Unlike a projection, it must never run again for an event it already handled, so it has its own stream
///         provider, <see cref="ProviderName"/>, with a durable checkpoint that starts from now. Rebuilding the
///         projections from the start of <c>$all</c> never replays the reactions.
///     </para>
///     <para>
///         The provider reads the streams of the grains whose events the orders react to and delivers each event, on the
///         Orleans stream namespace <c>{ProviderName}:{Category}</c>, to the streams it names as stream targets. The
///         order grain subscribes implicitly to <see cref="OrderNamespace"/>, keyed by the order ID, so it receives the
///         events that target it.
///     </para>
/// </remarks>
public static class OrderReactors
{
    /// <summary>The name of the stream provider that delivers the events the orders react to.</summary>
    public const string ProviderName = "OrderReactors";

    /// <summary>The category of an order's stream, <c>order-{TenantId}|{OrderGuid}</c>, the order grain's type.</summary>
    public const string OrderCategory = "order";

    /// <summary>The Orleans stream namespace the provider delivers the events that target an order on.</summary>
    public const string OrderNamespace = ProviderName + ":" + OrderCategory;
}
