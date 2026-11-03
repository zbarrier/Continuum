using System.Collections.Concurrent;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Providers.Streams.Common;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;
using Orleans.Statistics;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Queue adapter that lets the Orleans PersistentStreamProvider use KurrentDB as its event queue, for both the
///     catch-up and persistent subscription strategies.
/// </summary>
public class KurrentDBQueueAdapter : IQueueAdapter, IQueueAdapterCache
{
    private readonly KurrentDBOptions _options;
    private readonly KurrentDBReceiverOptionsBase _receiverOptions;
    private readonly IKurrentDBReceiverFactory _strategy;
    private readonly HashRingBasedPartitionedStreamQueueMapper _streamQueueMapper;
    private readonly Func<string, ILoggerFactory, IKurrentDBQueueCache> _cacheFactory;
    private readonly Func<KurrentDBReceiverMonitorDimensions, ILoggerFactory, IQueueAdapterReceiverMonitor> _receiverMonitorFactory;
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironmentStatisticsProvider? _environmentStatisticsProvider;
    private readonly Func<KurrentDBReceiverSettings, ILogger, IKurrentDBReceiver> _receiverFactory;
    private readonly LoadSheddingOptions _loadSheddingOptions;
    private readonly string _consumerGroup;

    private readonly ConcurrentDictionary<QueueId, IKurrentDBProducer> _producers = new();
    private readonly ConcurrentDictionary<QueueId, IKurrentDBQueueAdapterReceiver> _receivers = new();

    /// <summary>
    ///     Initializes a new instance of the KurrentDBQueueAdapter class.
    /// </summary>
    /// <param name="name">The name of the adapter.</param>
    /// <param name="options">The KurrentDB options.</param>
    /// <param name="strategy">The selected subscription strategy, which supplies the receiver options and builds each queue's receiver.</param>
    /// <param name="streamQueueMapper">The stream queue mapper.</param>
    /// <param name="cacheFactory">The cache factory.</param>
    /// <param name="receiverMonitorFactory">The receiver monitor factory.</param>
    /// <param name="receiverFactory">The KurrentDB receiver factory.</param>
    /// <param name="dataAdapter">The KurrentDB data adapter.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="environmentStatisticsProvider">The environment statistics provider.</param>
    public KurrentDBQueueAdapter(string name, KurrentDBOptions options, IKurrentDBReceiverFactory strategy, HashRingBasedPartitionedStreamQueueMapper streamQueueMapper, Func<string, ILoggerFactory, IKurrentDBQueueCache> cacheFactory, Func<KurrentDBReceiverMonitorDimensions, ILoggerFactory, IQueueAdapterReceiverMonitor> receiverMonitorFactory, Func<KurrentDBReceiverSettings, ILogger, IKurrentDBReceiver> receiverFactory, IKurrentDBDataAdapter dataAdapter, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IEnvironmentStatisticsProvider? environmentStatisticsProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(strategy, nameof(strategy));
        ArgumentNullException.ThrowIfNull(streamQueueMapper, nameof(streamQueueMapper));
        ArgumentNullException.ThrowIfNull(cacheFactory, nameof(cacheFactory));
        ArgumentNullException.ThrowIfNull(receiverMonitorFactory, nameof(receiverMonitorFactory));
        ArgumentNullException.ThrowIfNull(receiverFactory, nameof(receiverFactory));
        ArgumentNullException.ThrowIfNull(dataAdapter, nameof(dataAdapter));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        Name = name;
        _options = options;
        _strategy = strategy;
        _receiverOptions = strategy.ReceiverOptions;
        Direction = strategy.Direction;
        _streamQueueMapper = streamQueueMapper;
        _cacheFactory = cacheFactory;
        _receiverMonitorFactory = receiverMonitorFactory;
        _receiverFactory = receiverFactory;
        _dataAdapter = dataAdapter;
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _environmentStatisticsProvider = environmentStatisticsProvider;
        // Resolved once here rather than per receiver, so missing configuration fails when the adapter is built.
        _loadSheddingOptions = serviceProvider.GetRequiredService<IOptions<LoadSheddingOptions>>().Value;
        _consumerGroup = $"{serviceProvider.GetProviderClusterOptions(name).Value.ServiceId}-{name}";
    }

    #region IQueueAdapter Implementation

    /// <summary>
    ///     Name of the adapter. Primarily for logging purposes
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Determines whether this is a rewindable stream adapter - supports subscribing from previous point in time.
    /// </summary>
    /// <returns>True if this is a rewindable stream adapter, false otherwise.</returns>
    public bool IsRewindable => true;

    /// <summary>
    ///     Direction of this queue adapter: Read, Write or ReadWrite.
    /// </summary>
    /// <returns>The direction in which this adapter provides data.</returns>
    public StreamProviderDirection Direction { get; }

    /// <summary>
    ///     Writes a set of events to the queue as a single batch associated with the provided streamId.
    /// </summary>
    /// <typeparam name="T">The queue element type.</typeparam>
    /// <param name="streamId">The stream identifier.</param>
    /// <param name="events">The events.</param>
    /// <param name="token">The token.</param>
    /// <param name="requestContext">The request context.</param>
    /// <returns>Task.</returns>
    /// <remarks>
    ///     The batch becomes one KurrentDB record per event, appended together so the whole batch commits or none of
    ///     it does. The records carry the transaction metadata that identifies them as one append.
    /// </remarks>
    public Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken? token, Dictionary<string, object>? requestContext)
    {
        if (Direction == StreamProviderDirection.ReadOnly)
        {
            // Catch-up subscriptions read streams owned by event sourced grains. Writing here would bypass those grains.
            throw new InvalidOperationException(FormattableString.Invariant($"Stream provider {Name} is read only. Its events are produced by event sourced grains, not by the stream provider."));
        }
        var records = _dataAdapter.ToQueueMessage(streamId, events, token, requestContext);
        if (records.Length == 0)
        {
            return Task.CompletedTask;
        }
        var queueId = _streamQueueMapper.GetQueueForStream(streamId);
        var producer = _producers.GetOrAdd(queueId, static (qid, instance) => instance._strategy.CreateProducer(instance._streamQueueMapper.QueueToPartition(qid), instance._loggerFactory), this);
        return producer.AppendAsync(records);
    }

    /// <summary>
    ///     Creates a queue receiver for the specified queueId
    /// </summary>
    /// <param name="queueId">The queue identifier.</param>
    /// <returns>The receiver.</returns>
    public IQueueAdapterReceiver CreateReceiver(QueueId queueId)
    {
        return GetOrCreateReceiver(queueId);
    }

    private IKurrentDBQueueAdapterReceiver GetOrCreateReceiver(QueueId queueId)
    {
        return _receivers.GetOrAdd(queueId, static (qid, instance) => instance.MakeReceiver(qid), this);
    }

    private IKurrentDBQueueAdapterReceiver MakeReceiver(QueueId queueId)
    {
        var receiverSettings = new KurrentDBReceiverSettings
                               {
                                   Options = _options,
                                   ReceiverOptions = _receiverOptions,
                                   ConsumerGroup = _consumerGroup,
                                   QueueName = _streamQueueMapper.QueueToPartition(queueId),
                                   StreamNameParser = _serviceProvider.GetKeyedService<Continuum.Streams.IStreamNameParser>(_options.ConnectionName)
                               };
        var receiverMonitorDimensions = new KurrentDBReceiverMonitorDimensions
                                        {
                                            QueueName = receiverSettings.QueueName,
                                            Name = receiverSettings.Options.Name
                                        };
        return _strategy.CreateQueueAdapterReceiver(new KurrentDBQueueAdapterReceiverContext
        {
            Settings = receiverSettings,
            CacheFactory = _cacheFactory,
            ReceiverFactory = _receiverFactory,
            LoggerFactory = _loggerFactory,
            ReceiverMonitor = _receiverMonitorFactory(receiverMonitorDimensions, _loggerFactory),
            LoadSheddingOptions = _loadSheddingOptions,
            EnvironmentStatisticsProvider = _environmentStatisticsProvider,
        });
    }

    #endregion

    #region IQueueAdapterCache Implementation

    /// <summary>
    ///     Create a cache for a given queue id.
    /// </summary>
    /// <param name="queueId">The queue id.</param>
    /// <returns>The queue cache..</returns>
    public IQueueCache CreateQueueCache(QueueId queueId)
    {
        return GetOrCreateReceiver(queueId);
    }

    #endregion

}
