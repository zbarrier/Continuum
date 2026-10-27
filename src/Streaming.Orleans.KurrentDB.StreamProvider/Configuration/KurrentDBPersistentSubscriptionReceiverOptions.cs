using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     Receiver options for the KurrentDB persistent subscription (consumer group) strategy.
/// </summary>
/// <remarks>
///     With this strategy KurrentDB owns the read position for each consumer group, so no client side checkpoint
///     store is involved. Select it with <c>UsePersistentSubscriptions</c> on the stream configurator.
///     <para>
///         Events are acknowledged as they are received, so events still in the queue cache are lost if the silo
///         stops unexpectedly, and ordering is not guaranteed. Use this strategy only for consumers that tolerate
///         dropped and out-of-order events; consumers that need ordering and no drops should use catch-up subscriptions.
///     </para>
/// </remarks>
public sealed class KurrentDBPersistentSubscriptionReceiverOptions : KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     The KurrentDB persistent subscription settings.
    /// </summary>
    /// <remarks>
    ///     This instance is shared by every receiver created for this stream provider and must be treated as immutable.
    ///     Receivers derive a per-queue copy with the resolved start position rather than mutating this instance.
    ///     <para>
    ///         Set <see cref="PersistentSubscriptionSettings.ConsumerStrategyName" /> to
    ///         <see cref="SystemConsumerStrategies.Pinned" /> to keep events from the same source stream on the same
    ///         consumer, which preserves per-stream order while the set of consumers is stable. Order can still break when
    ///         queues move between silos, silos start or stop, or events are retried, so Pinned is best effort only.
    ///     </para>
    /// </remarks>
    public PersistentSubscriptionSettings SubscriptionSettings { get; set; } = new();

    /// <summary>
    ///     Indicates whether an already existing persistent subscription should be updated with
    ///     <see cref="SubscriptionSettings" /> when the receiver initializes.
    /// </summary>
    /// <remarks>
    ///     Updating an existing persistent subscription resets its server side start position, which discards the
    ///     consumer group's progress. This is off by default so that restarts resume where the consumer group left off.
    /// </remarks>
    public bool UpdateExistingSubscription { get; set; } = false;

    /// <summary>
    ///     Optional parameter that configures the receiver prefetch count.
    /// </summary>
    public int PrefetchCount { get; set; } = 10;
}
