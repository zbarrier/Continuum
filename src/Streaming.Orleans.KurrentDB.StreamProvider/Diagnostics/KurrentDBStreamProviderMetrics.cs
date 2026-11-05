using System.Diagnostics.Metrics;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Diagnostics;

/// <summary>
///     Continuum metrics for KurrentDB stream provider queue receivers. Standard receiver, cache and block pool metrics
///     are reported by the Orleans monitors.
/// </summary>
public sealed class KurrentDBStreamProviderMetrics : IDisposable
{
    /// <summary>
    ///     The name of the meter.
    /// </summary>
    public const string MeterName = "Continuum.Streaming.Orleans.KurrentDB.StreamProvider";

    private const string ProviderTag = "continuum.stream_provider.name";
    private const string QueueTag = "continuum.queue.name";
    private const string StateTag = "continuum.queue.receiver.state";

    private static readonly KeyValuePair<string, object?> DbSystemTag = new("db.system.name", "kurrentdb");

    private static readonly (KurrentDBQueueReceiverState State, string Name)[] States =
        [.. Enum.GetValues<KurrentDBQueueReceiverState>().Select(state => (state, state.ToString().ToLowerInvariant()))];

    private readonly Meter _meter;
    private readonly bool _ownsMeter;
    private readonly Counter<long> _reconnects;
    private readonly Counter<long> _initializationFailures;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBStreamProviderMetrics" /> class.
    /// </summary>
    /// <param name="statusRegistry">The receiver status registry observed by the state metric.</param>
    /// <param name="meterFactory">The meter factory, or <see langword="null" /> to create an owned meter.</param>
    public KurrentDBStreamProviderMetrics(KurrentDBQueueReceiverStatusRegistry statusRegistry, IMeterFactory? meterFactory = null)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);

        _ownsMeter = meterFactory is null;
        _meter = meterFactory?.Create(MeterName) ?? new Meter(MeterName);

        _reconnects = _meter.CreateCounter<long>("continuum.queue.receiver.reconnects", "{reconnect}",
            "Reconnects after the subscription dropped.");
        _initializationFailures = _meter.CreateCounter<long>("continuum.queue.receiver.initialization.failures", "{failure}",
            "Receiver initializations that failed.");
        _meter.CreateObservableUpDownCounter("continuum.queue.receiver.state",
            () => statusRegistry.Statuses.SelectMany(status => States.Select(state =>
                new Measurement<int>(state.State == status.State ? 1 : 0, DbSystemTag,
                    new KeyValuePair<string, object?>(ProviderTag, status.ProviderName),
                    new KeyValuePair<string, object?>(QueueTag, status.QueueName),
                    new KeyValuePair<string, object?>(StateTag, state.Name)))),
            "{receiver}",
            "Whether the receiver is in the state given by the continuum.queue.receiver.state attribute: 1 if it is, 0 otherwise.");
    }

    /// <summary>
    ///     Records a reconnect after the subscription dropped.
    /// </summary>
    public void Reconnected(string providerName, string queueName) => _reconnects.Add(1, DbSystemTag,
        new KeyValuePair<string, object?>(ProviderTag, providerName), new KeyValuePair<string, object?>(QueueTag, queueName));

    /// <summary>
    ///     Records a failed receiver initialization.
    /// </summary>
    public void InitializationFailed(string providerName, string queueName) => _initializationFailures.Add(1, DbSystemTag,
        new KeyValuePair<string, object?>(ProviderTag, providerName), new KeyValuePair<string, object?>(QueueTag, queueName));

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsMeter)
        {
            _meter.Dispose();
        }
    }
}
