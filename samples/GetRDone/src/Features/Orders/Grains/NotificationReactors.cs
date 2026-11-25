namespace GetRDone.Orders.Grains;

/// <summary>
///     Names shared by the notifications that go out when something happens to an order and the silo that hosts them.
/// </summary>
/// <remarks>
///     <para>
///         A notification is a reaction: it emails someone when an event happens, such as telling the AMC about an order
///         that was given its number. Like the <see cref="OrderReactors"/>, it must never run again for an event it
///         already handled, so its stream provider, <see cref="ProviderName"/>, has a durable checkpoint that starts from
///         now.
///     </para>
///     <para>
///         It has its own provider because it reacts to the order's own events, while <see cref="OrderReactors"/> reads
///         only the streams of the grains whose events target an order. The provider reads the order streams and delivers
///         each event, on the Orleans stream namespace <see cref="OrderNamespace"/>, to the stream of the order it was
///         appended to, keyed by the order ID, so each order's notifier receives its order's events.
///     </para>
/// </remarks>
public static class NotificationReactors
{
    /// <summary>The name of the stream provider that delivers the events the notifications react to.</summary>
    public const string ProviderName = "NotificationReactors";

    /// <summary>The category of an order's stream, <c>order-{TenantId}|{OrderGuid}</c>.</summary>
    public const string OrderCategory = OrderReactors.OrderCategory;

    /// <summary>The Orleans stream namespace the provider delivers an order's events on.</summary>
    public const string OrderNamespace = ProviderName + ":" + OrderCategory;
}
