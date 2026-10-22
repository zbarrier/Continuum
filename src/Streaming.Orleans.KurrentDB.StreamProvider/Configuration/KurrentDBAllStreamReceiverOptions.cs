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
public sealed class KurrentDBAllStreamReceiverOptions : KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     Indicates whether link events should be resolved to the events they point at.
    /// </summary>
    public bool ResolveLinkTos { get; set; } = true;

    /// <summary>
    ///     An optional server side stream name prefix filter applied to the <c>$all</c> subscription.
    /// </summary>
    public string? StreamFilterPrefix { get; set; }

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
