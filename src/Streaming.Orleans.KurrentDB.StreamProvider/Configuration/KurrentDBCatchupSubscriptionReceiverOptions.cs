using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

namespace Orleans.Configuration;

/// <summary>
///     Receiver options shared by the KurrentDB catch-up subscription strategies.
/// </summary>
/// <remarks>
///     <para>
///         With these strategies the client owns the read position, which is persisted through
///         <see cref="CheckpointStore" /> and throttled by <see cref="CheckpointMonitorOptions" />.
///     </para>
///     <para>
///         A catch-up subscription reads one source, either <c>$all</c> or a single stream, so
///         <see cref="KurrentDBOptions.Queues" /> is validated to contain exactly one entry, which names that source.
///         The checkpoint is stored under the subscription name, and a subscription name is permanently bound to its
///         source and checkpoint. To replay, or to change the source, use a new consumer group (for example
///         <c>Projections-V2</c>), which starts with no checkpoint.
///     </para>
/// </remarks>
public abstract class KurrentDBCatchupSubscriptionReceiverOptions : KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     Indicates whether link events should be resolved to the events they point at.
    /// </summary>
    /// <remarks>
    ///     A resolved link is delivered as the event it points at, at the link's own position. Resolving links is
    ///     required to receive the events of <c>$ce-</c> and other projected link streams.
    /// </remarks>
    public bool ResolveLinkTos { get; set; } = true;

    /// <summary>
    ///     The number of Orleans reads, of up to 1000 events each, that can be read and held in memory
    ///     ahead of the cache.
    /// </summary>
    /// <remarks>
    ///     When the buffer is full the subscription waits for Orleans to drain it, so a catch-up from the start does
    ///     not read the whole source into memory. A larger buffer smooths bursts at the cost of memory.
    /// </remarks>
    public int BufferCapacityMultiplier { get; set; } = 10;

    /// <summary>
    ///     How long to wait for the rest of a transaction after its last event arrived before delivering the events read
    ///     as a partial transaction.
    /// </summary>
    /// <remarks>
    ///     A subscription filter can exclude part of a transaction, and the remaining events never arrive. Their arrival
    ///     is otherwise only ruled out by the next event, so on a quiet subscription the events read would wait until
    ///     more are written. A partial transaction is logged as a warning in case the filter was not meant to exclude it.
    /// </remarks>
    public TimeSpan IncompleteTransactionTimeout { get; set; } = TimeSpan.FromSeconds(60);

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
    ///     The store used to persist the read position.
    ///     Resolved from <see cref="CheckpointConnectionName" /> when not explicitly configured.
    /// </summary>
    [Redact]
    public ICheckpointStore<ulong> CheckpointStore { get; set; } = default!;
}
