namespace Continuum.Streaming.Orleans;

/// <summary>
/// Holds the checkpoint of a single subscription, persisted through Orleans grain storage.
/// </summary>
/// <typeparam name="TPosition">The provider-specific stream position type.</typeparam>
/// <remarks>
/// The grain key identifies the subscription. Use <see cref="SubscriptionCheckpointStore{TPosition}"/> rather than
/// calling the grain directly, so keys are built consistently.
/// </remarks>
[Alias("Continuum.Streaming.ISubscriptionCheckpointGrain.V1`1")]
public interface ISubscriptionCheckpointGrain<TPosition> : IGrainWithStringKey
    where TPosition : IComparable<TPosition>
{
    /// <summary>
    /// Gets the last checkpoint, including one stored but not yet written to grain storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The last checkpoint, or the default position if none has been stored.</returns>
    [Alias("GetCheckpoint")]
    Task<TPosition> GetCheckpointAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the checkpoint in memory. It is written to grain storage periodically and when the grain deactivates.
    /// </summary>
    /// <param name="position">The processed position. Positions at or behind the current checkpoint are ignored.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes once the checkpoint is recorded in memory.</returns>
    [Alias("StoreCheckpoint")]
    Task StoreCheckpointAsync(TPosition position, CancellationToken cancellationToken = default);
}
