using System.Collections.Concurrent;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;

/// <summary>
///     The state of a catch-up subscription.
/// </summary>
public enum KurrentDBCatchupSubscriptionState
{
    /// <summary>The subscription is starting and has not yet connected.</summary>
    Starting = 0,
    /// <summary>The subscription is connected and processing events.</summary>
    Running = 1,
    /// <summary>The subscription dropped and is waiting to reconnect.</summary>
    Reconnecting = 2,
    /// <summary>A batch failed with a transient error and is being retried.</summary>
    Retrying = 3,
    /// <summary>The subscription stopped because a batch failed with a non-transient error.</summary>
    Faulted = 4,
    /// <summary>The subscription was stopped by the host.</summary>
    Stopped = 5
}

/// <summary>
///     A snapshot of a catch-up subscription's status.
/// </summary>
/// <param name="SubscriptionName">The subscription name.</param>
/// <param name="State">The subscription state.</param>
/// <param name="Description">Details about the state, such as the failed batch, or <see langword="null" />.</param>
/// <param name="Exception">The error behind the state, or <see langword="null" />.</param>
public sealed record KurrentDBCatchupSubscriptionStatus(string SubscriptionName, KurrentDBCatchupSubscriptionState State,
    string? Description = null, Exception? Exception = null);

/// <summary>
///     Tracks the status of every catch-up subscription in the process.
/// </summary>
public sealed class KurrentDBCatchupSubscriptionStatusRegistry
{
    private readonly ConcurrentDictionary<string, KurrentDBCatchupSubscriptionStatus> _statuses = new(StringComparer.Ordinal);

    /// <summary>
    ///     Gets the status of every subscription that has reported one.
    /// </summary>
    public IReadOnlyCollection<KurrentDBCatchupSubscriptionStatus> Statuses => _statuses.Values.ToArray();

    /// <summary>
    ///     Records a subscription's status.
    /// </summary>
    /// <param name="status">The status.</param>
    public void Report(KurrentDBCatchupSubscriptionStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        _statuses[status.SubscriptionName] = status;
    }

    /// <summary>
    ///     Gets a subscription's status.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <param name="status">The status, if reported.</param>
    /// <returns><see langword="true" /> if the subscription has reported a status.</returns>
    public bool TryGetStatus(string subscriptionName, out KurrentDBCatchupSubscriptionStatus? status)
    {
        return _statuses.TryGetValue(subscriptionName, out status);
    }
}
