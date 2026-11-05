using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     Creates receivers backed by a client checkpointed KurrentDB catch-up subscription to <c>$all</c> or to a single
///     stream.
/// </summary>
/// <remarks>
///     The read position is owned by the client, so this strategy is validated to a single queue and resolves the
///     shared keyed <see cref="KurrentDBClient" /> for the configured connection.
/// </remarks>
public sealed class KurrentDBCatchupSubscriptionReceiverFactory : IKurrentDBReceiverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KurrentDBCatchupSubscriptionReceiverOptions _receiverOptions;
    private readonly string _connectionName;
    private readonly KurrentDBCheckpointMarkerData _checkpointMarker;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBCatchupSubscriptionReceiverFactory" />.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve the keyed client.</param>
    /// <param name="name">The stream provider name.</param>
    /// <param name="receiverOptions">
    ///     The receiver options. Their type selects the source: <see cref="KurrentDBAllStreamCatchupSubscriptionReceiverOptions" />
    ///     subscribes to <c>$all</c>, and <see cref="KurrentDBStreamCatchupSubscriptionReceiverOptions" /> to the stream named
    ///     by the queue.
    /// </param>
    public KurrentDBCatchupSubscriptionReceiverFactory(IServiceProvider serviceProvider, string name, KurrentDBCatchupSubscriptionReceiverOptions receiverOptions)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        _serviceProvider = serviceProvider;
        ArgumentNullException.ThrowIfNull(receiverOptions, nameof(receiverOptions));
        _receiverOptions = receiverOptions;
        _connectionName = serviceProvider.GetOptionsByName<KurrentDBOptions>(name).ConnectionName;
        _checkpointMarker = KurrentDBCheckpointMarkerData.Create(serviceProvider.GetOptionsByName<KurrentDBDataAdapterOptions>(name));
    }

    /// <inheritdoc />
    public KurrentDBReceiverOptionsBase ReceiverOptions => _receiverOptions;

    /// <inheritdoc />
    public IKurrentDBQueueCache CreateQueueCache(KurrentDBQueueCacheContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var logger = context.CreateLogger<KurrentDBCatchupQueueCache>();
        // The client owns the checkpoint, so only events every cursor has finished with may be purged and checkpointed.
        var cursorTracker = new KurrentDBCursorTracker();
        var evictionStrategy = new KurrentDBCatchupEvictionStrategy(logger, context.TimePurge, context.CacheMonitor, context.StatisticMonitorWriteInterval, context.DataAdapter, cursorTracker);
        return new KurrentDBCatchupQueueCache(context.Queue, KurrentDBCatchupQueueAdapterReceiver.MaxMessagesPerRead, context.BufferPool, context.DataAdapter, evictionStrategy, cursorTracker, logger, context.CacheMonitor, context.StatisticMonitorWriteInterval, context.MetadataMinTimeInCache);
    }

    /// <inheritdoc />
    public StreamProviderDirection Direction => StreamProviderDirection.ReadOnly;

    /// <inheritdoc />
    public IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, ILogger logger)
    {
        // The client is a keyed singleton shared by every queue of this connection. It is owned by the container, so
        // receivers must not dispose it.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_connectionName);
        return new KurrentDBCatchupSubscriptionReceiver(client, settings, logger, _checkpointMarker);
    }

    /// <inheritdoc />
    public IKurrentDBQueueAdapterReceiver CreateQueueAdapterReceiver(KurrentDBQueueAdapterReceiverContext context) => new KurrentDBCatchupQueueAdapterReceiver(context);

    /// <inheritdoc />
    public IKurrentDBProducer CreateProducer(string queueName, ILoggerFactory loggerFactory) =>
        throw new NotSupportedException("Catch-up subscriptions are read only. Their events are produced by event sourced grains, not by the stream provider.");
}
