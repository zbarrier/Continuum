namespace Continuum.Streaming;

/// <summary>
/// A named subscription to a stream.
/// </summary>
public interface IStreamSubscription
{
    /// <summary>
    /// Gets the subscription name, which also identifies its checkpoint.
    /// </summary>
    string SubscriptionName { get; }
}

/// <summary>
/// A stream subscription whose lifetime is managed by a host.
/// </summary>
public interface IHostedStreamSubscription : IStreamSubscription
{
    /// <summary>
    /// Starts the subscription.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the subscription has started.</returns>
    Task StartAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Stops the subscription.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the subscription has stopped.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}
