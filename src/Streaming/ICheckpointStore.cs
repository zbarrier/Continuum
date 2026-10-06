namespace Continuum.Streaming;

/// <summary>
/// Persists the last processed position of named subscriptions.
/// </summary>
/// <typeparam name="TStreamPosition">The provider-specific stream position type.</typeparam>
public interface ICheckpointStore<TStreamPosition>
{
    /// <summary>
    /// Gets the last stored position of the subscription.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The last stored position.</returns>
    Task<TStreamPosition> GetLastCheckpointAsync(string subscriptionName, CancellationToken cancellationToken = default);
    /// <summary>
    /// Stores the position the subscription has processed up to.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <param name="streamPosition">The processed position.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the checkpoint is stored.</returns>
    Task StoreCheckpointAsync(string subscriptionName, TStreamPosition streamPosition, CancellationToken cancellationToken = default);
}
