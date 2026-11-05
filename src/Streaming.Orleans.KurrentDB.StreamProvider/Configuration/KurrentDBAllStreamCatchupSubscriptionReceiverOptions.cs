using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     Receiver options for the KurrentDB <c>$all</c> catch-up subscription strategy.
/// </summary>
/// <remarks>
///     <see cref="KurrentDBOptions.Queues" /> must contain exactly one entry, <c>$all</c>.
/// </remarks>
public sealed class KurrentDBAllStreamCatchupSubscriptionReceiverOptions : KurrentDBCatchupSubscriptionReceiverOptions
{
    /// <summary>
    ///     The server side filter applied to the <c>$all</c> subscription, such as
    ///     <see cref="StreamFilter.Prefix(string[])" /> or <see cref="EventTypeFilter.Prefix(string[])" />. Defaults to
    ///     <see cref="EventTypeFilter.ExcludeSystemEvents" />, which skips <c>$</c>-prefixed system events such as
    ///     link events, metadata and checkpoints. Set to <see langword="null" /> to read every event. Prefer prefix
    ///     filters; regular expression filters are evaluated against every event in <c>$all</c>.
    /// </summary>
    /// <remarks>
    ///     When <see cref="KurrentDBCatchupSubscriptionReceiverOptions.ResolveLinkTos" /> is set and the filter matches
    ///     both an event and a link to it, the event is delivered once for each, so filter to the original streams or to
    ///     the link streams, not both.
    /// </remarks>
    public IEventFilter? Filter { get; set; } = EventTypeFilter.ExcludeSystemEvents();
}
