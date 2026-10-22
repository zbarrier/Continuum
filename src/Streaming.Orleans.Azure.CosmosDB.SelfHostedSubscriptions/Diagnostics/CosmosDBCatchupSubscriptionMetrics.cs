using System.Diagnostics.Metrics;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;

/// <summary>
///     Records metrics for catch-up subscriptions. Collect them with the meter named <see cref="MeterName" />, for
///     example with OpenTelemetry's <c>AddMeter</c>.
/// </summary>
public sealed class CosmosDBCatchupSubscriptionMetrics : IDisposable
{
    /// <summary>
    ///     The name of the meter the metrics are recorded with.
    /// </summary>
    public const string MeterName = "Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions";

    private const string SubscriptionTag = "continuum.subscription.name";
    private const string StateTag = "continuum.subscription.state";

    private static readonly KeyValuePair<string, object?> DbSystemTag = new("db.system.name", "azure.cosmosdb");

    private static readonly (CosmosDBCatchupSubscriptionState State, string Name)[] States =
        [.. Enum.GetValues<CosmosDBCatchupSubscriptionState>().Select(state => (state, state.ToString().ToLowerInvariant()))];

    private readonly Meter _meter;
    private readonly bool _ownsMeter;
    private readonly Counter<long> _eventsProcessed;
    private readonly Counter<long> _batchesProcessed;
    private readonly Counter<long> _batchRetries;
    private readonly Counter<long> _errors;
    private readonly Histogram<double> _batchDuration;

    /// <summary>
    ///     Initializes a new instance of the metrics.
    /// </summary>
    /// <param name="statusRegistry">The registry the subscription state gauge is read from.</param>
    /// <param name="meterFactory">The factory the meter is created with, or <see langword="null" /> to create one directly.</param>
    public CosmosDBCatchupSubscriptionMetrics(CosmosDBCatchupSubscriptionStatusRegistry statusRegistry, IMeterFactory? meterFactory = null)
    {
        ArgumentNullException.ThrowIfNull(statusRegistry);

        _ownsMeter = meterFactory is null;
        _meter = meterFactory?.Create(MeterName) ?? new Meter(MeterName);

        _eventsProcessed = _meter.CreateCounter<long>("continuum.subscription.events.processed", "{event}",
            "Events handled successfully.");
        _batchesProcessed = _meter.CreateCounter<long>("continuum.subscription.batches.processed", "{batch}",
            "Batches handled successfully.");
        _batchRetries = _meter.CreateCounter<long>("continuum.subscription.batch.retries", "{retry}",
            "Batches that failed in the handler and will be retried by the change feed processor.");
        _errors = _meter.CreateCounter<long>("continuum.subscription.errors", "{error}",
            "Change feed processor errors not raised by the handler, such as lost leases.");
        _batchDuration = _meter.CreateHistogram<double>("continuum.subscription.batch.duration", "s",
            "Time taken to handle a batch successfully.");
        _meter.CreateObservableUpDownCounter("continuum.subscription.state",
            () => statusRegistry.Statuses.SelectMany(status => States.Select(state =>
                new Measurement<int>(state.State == status.State ? 1 : 0, DbSystemTag,
                    new KeyValuePair<string, object?>(SubscriptionTag, status.SubscriptionName),
                    new KeyValuePair<string, object?>(StateTag, state.Name)))),
            "{subscription}",
            "Whether the subscription is in the state given by the continuum.subscription.state attribute: 1 if it is, 0 otherwise.");
    }

    /// <summary>
    ///     Records a successfully handled batch.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    /// <param name="eventCount">The number of events in the batch.</param>
    /// <param name="duration">The time taken to handle the batch.</param>
    public void BatchProcessed(string subscriptionName, int eventCount, TimeSpan duration)
    {
        var subscriptionTag = new KeyValuePair<string, object?>(SubscriptionTag, subscriptionName);
        _eventsProcessed.Add(eventCount, DbSystemTag, subscriptionTag);
        _batchesProcessed.Add(1, DbSystemTag, subscriptionTag);
        _batchDuration.Record(duration.TotalSeconds, DbSystemTag, subscriptionTag);
    }

    /// <summary>
    ///     Records a batch that failed in the handler and will be retried.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    public void BatchRetried(string subscriptionName) => _batchRetries.Add(1, DbSystemTag, new KeyValuePair<string, object?>(SubscriptionTag, subscriptionName));

    /// <summary>
    ///     Records a change feed processor error not raised by the handler.
    /// </summary>
    /// <param name="subscriptionName">The subscription name.</param>
    public void Error(string subscriptionName) => _errors.Add(1, DbSystemTag, new KeyValuePair<string, object?>(SubscriptionTag, subscriptionName));

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsMeter)
        {
            _meter.Dispose();
        }
    }
}
