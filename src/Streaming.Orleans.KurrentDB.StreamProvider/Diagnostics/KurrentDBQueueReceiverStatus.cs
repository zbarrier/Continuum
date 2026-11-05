using System.Collections.Concurrent;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

/// <summary>
///     The lifecycle state of a stream provider queue receiver.
/// </summary>
public enum KurrentDBQueueReceiverState
{
    /// <summary>The receiver is connecting to KurrentDB.</summary>
    Starting = 0,

    /// <summary>The receiver is subscribed and receiving events.</summary>
    Running = 1,

    /// <summary>The subscription dropped and the receiver is reconnecting.</summary>
    Reconnecting = 2,

    /// <summary>The receiver failed to start. Orleans retries it on the next read.</summary>
    Faulted = 3
}

/// <summary>
///     The status of one queue receiver of a KurrentDB stream provider on this silo.
/// </summary>
/// <param name="ProviderName">The stream provider name.</param>
/// <param name="QueueName">The queue name, which is the KurrentDB stream or partition read by the receiver.</param>
/// <param name="State">The receiver state.</param>
/// <param name="Description">A description of the state, or <see langword="null" />.</param>
/// <param name="Exception">The exception that caused the state, or <see langword="null" />.</param>
public sealed record KurrentDBQueueReceiverStatus(string ProviderName, string QueueName, KurrentDBQueueReceiverState State,
    string? Description = null, Exception? Exception = null);

/// <summary>
///     Holds the status of every KurrentDB stream provider queue receiver running on this silo.
/// </summary>
/// <remarks>
///     A receiver is removed when it shuts down, since Orleans moves queues between silos as the cluster changes.
/// </remarks>
public sealed class KurrentDBQueueReceiverStatusRegistry
{
    private readonly ConcurrentDictionary<(string ProviderName, string QueueName), KurrentDBQueueReceiverStatus> _statuses = new();

    /// <summary>
    ///     The status of every receiver running on this silo.
    /// </summary>
    public IReadOnlyCollection<KurrentDBQueueReceiverStatus> Statuses => _statuses.Values.ToArray();

    /// <summary>
    ///     Records the status of a receiver.
    /// </summary>
    public void Report(KurrentDBQueueReceiverStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);
        _statuses[(status.ProviderName, status.QueueName)] = status;
    }

    /// <summary>
    ///     Removes a receiver that has shut down.
    /// </summary>
    public void Remove(string providerName, string queueName)
    {
        _statuses.TryRemove((providerName, queueName), out _);
    }

    /// <summary>
    ///     Gets the status of a receiver.
    /// </summary>
    public bool TryGetStatus(string providerName, string queueName, out KurrentDBQueueReceiverStatus? status)
    {
        return _statuses.TryGetValue((providerName, queueName), out status);
    }
}
