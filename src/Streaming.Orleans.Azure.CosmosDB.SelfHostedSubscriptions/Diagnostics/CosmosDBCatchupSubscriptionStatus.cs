using System.Collections.Concurrent;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

/// <summary>
///     The state of a catch-up subscription.
/// </summary>
public enum CosmosDBCatchupSubscriptionState
{
    /// <summary>The subscription is starting the change feed processor.</summary>
    Starting = 0,
    /// <summary>The change feed processor is running and batches are being handled.</summary>
    Running = 1,
    /// <summary>A batch failed in the handler and the change feed processor is retrying it.</summary>
    Retrying = 2,
    /// <summary>The subscription was stopped by the host.</summary>
    Stopped = 3
}

/// <summary>
///     A snapshot of a catch-up subscription's status.
/// </summary>
/// <param name="SubscriptionName">The subscription name.</param>
/// <param name="State">The subscription state.</param>
/// <param name="Description">Details about the state, such as the failing lease, or <see langword="null" />.</param>
/// <param name="Exception">The error behind the state, or <see langword="null" />.</param>
public sealed record CosmosDBCatchupSubscriptionStatus(string SubscriptionName, CosmosDBCatchupSubscriptionState State,
    string? Description = null, Exception? Exception = null);

/// <summary>
///     Tracks the status of every catch-up subscription in the process.
/// </summary>
public sealed class CosmosDBCatchupSubscriptionStatusRegistry
{
    private readonly ConcurrentDictionary<string, CosmosDBCatchupSubscriptionStatus> _statuses = new(StringComparer.Ordinal);

    /// <summary>
    ///     Gets the status of every subscription that has reported one.
    /// </summary>
    public IReadOnlyCollection<CosmosDBCatchupSubscriptionStatus> Statuses => _statuses.Values.ToArray();

    /// <summary>
    ///     Records a subscription's status.
    /// </summary>
    /// <param name="status">The status.</param>
    public void Report(CosmosDBCatchupSubscriptionStatus status)
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
    public bool TryGetStatus(string subscriptionName, out CosmosDBCatchupSubscriptionStatus? status)
    {
        return _statuses.TryGetValue(subscriptionName, out status);
    }
}
