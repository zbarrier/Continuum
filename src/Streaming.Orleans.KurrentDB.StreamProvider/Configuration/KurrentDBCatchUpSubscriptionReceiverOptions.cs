using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

namespace Orleans.Configuration;

/// <summary>
///     Receiver options for the KurrentDB <c>$all</c> catch-up subscription strategy.
/// </summary>
/// <remarks>
///     <para>
///         With this strategy the client owns the read position, which is persisted through
///         <see cref="CheckpointStore" /> and throttled by <see cref="CheckpointMonitorOptions" />.
///     </para>
///     <para>
///         A <c>$all</c> subscription is inherently global. Orleans creates one receiver per queue, so more than one
///         queue would subscribe to <c>$all</c> multiple times and deliver every event once per queue while contending
///         over a single checkpoint stream. <see cref="KurrentDBOptions.Queues" /> is therefore validated to contain
///         exactly one entry when this strategy is selected.
///     </para>
/// </remarks>
public sealed class KurrentDBCatchUpSubscriptionReceiverOptions : KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     Indicates whether link events should be resolved to the events they point at.
    /// </summary>
    /// <remarks>
    ///     A resolved link is delivered as the event it points at, at the link's own <c>$all</c> position. When
    ///     <see cref="StreamFilterPrefix" /> matches both an event's stream and a stream linking to it, the event is
    ///     delivered once for each, so filter to the original streams or to the link streams, not both.
    /// </remarks>
    public bool ResolveLinkTos { get; set; } = true;

    /// <summary>
    ///     An optional server side stream name prefix filter applied to the <c>$all</c> subscription.
    /// </summary>
    public string? StreamFilterPrefix { get; set; }

    /// <summary>
    ///     The number of Orleans reads, of up to 1000 events each, that can be read from <c>$all</c> and held in memory
    ///     ahead of the cache.
    /// </summary>
    /// <remarks>
    ///     When the buffer is full the subscription waits for Orleans to drain it, so a catch-up from the start of
    ///     <c>$all</c> does not read the whole log into memory. A larger buffer smooths bursts at the cost of memory.
    /// </remarks>
    public int BufferCapacityMultiplier { get; set; } = 10;

    /// <summary>
    ///     The name of the KurrentDB connection used to resolve the <see cref="CheckpointStore" />.
    ///     When not set, the unkeyed <see cref="ICheckpointStore{T}" /> registration is used.
    /// </summary>
    [Redact]
    public string? CheckpointConnectionName { get; set; }

    /// <summary>
    ///     Controls how frequently the read position is committed to the <see cref="CheckpointStore" />.
    /// </summary>
    public KurrentDBCheckpointMonitorOptions CheckpointMonitorOptions { get; set; } = new();

    /// <summary>
    ///     The store used to persist the <c>$all</c> read position.
    ///     Resolved from <see cref="CheckpointConnectionName" /> when not explicitly configured.
    /// </summary>
    [Redact]
    public ICheckpointStore<ulong> CheckpointStore { get; set; } = default!;
}
