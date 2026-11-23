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
