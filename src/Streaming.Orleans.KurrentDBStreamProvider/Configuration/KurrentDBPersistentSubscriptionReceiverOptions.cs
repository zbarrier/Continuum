using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     Receiver options for the KurrentDB persistent subscription (consumer group) strategy.
/// </summary>
/// <remarks>
///     With this strategy KurrentDB owns the read position for each consumer group, so no client side checkpoint
///     store is involved. Select it with <c>UsePersistentSubscriptions</c> on the stream configurator.
/// </remarks>
public sealed class KurrentDBPersistentSubscriptionReceiverOptions : KurrentDBReceiverOptionsBase
{
    /// <summary>
    ///     The KurrentDB persistent subscription settings.
    /// </summary>
    /// <remarks>
    ///     This instance is shared by every receiver created for this stream provider and must be treated as immutable.
    ///     Receivers derive a per-queue copy with the resolved start position rather than mutating this instance.
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
