using System.Collections.Concurrent;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Providers.Streams.Common;
using Orleans.Providers.Streams.KurrentDB.StatisticMonitors;
using Orleans.Statistics;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Queue adapter factory which allows the PersistentStreamProvider to use KurrentDB as its backend persistent event queue.
/// </summary>
public class KurrentDBQueueAdapter : IQueueAdapter, IQueueAdapterCache
{
    private readonly KurrentDBOptions _options;
    private readonly KurrentDBReceiverOptionsBase _receiverOptions;
    private readonly HashRingBasedPartitionedStreamQueueMapper _streamQueueMapper;
    private readonly Func<string, IStreamQueueCheckpointer<string>, ILoggerFactory, IKurrentDBQueueCache> _cacheFactory;
    private readonly Func<KurrentDBReceiverMonitorDimensions, ILoggerFactory, IQueueAdapterReceiverMonitor> _receiverMonitorFactory;
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironmentStatisticsProvider? _environmentStatisticsProvider;
    private readonly Func<KurrentDBReceiverSettings, string, ILogger, IKurrentDBReceiver>? _receiverFactory;

    private IStreamQueueCheckpointerFactory? _checkpointerFactory;

    private readonly ConcurrentDictionary<QueueId, KurrentDBProducer> _producers = new();
    private readonly ConcurrentDictionary<QueueId, KurrentDBQueueAdapterReceiver> _receivers = new();

    /// <summary>
    ///     Initializes a new instance of the KurrentDBQueueAdapter class.
    /// </summary>
    /// <param name="name">The name of the adapter.</param>
    /// <param name="options">The KurrentDB options.</param>
    /// <param name="receiverOptions">The KurrentDB receiver options.</param>
    /// <param name="streamQueueMapper">The stream queue mapper.</param>
    /// <param name="cacheFactory">The cache factory.</param>
    /// <param name="receiverMonitorFactory">The receiver monitor factory.</param>
    /// <param name="receiverFactory">The KurrentDB receiver factory.</param>
    /// <param name="dataAdapter">The KurrentDB data adapter.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="environmentStatisticsProvider">The environment statistics provider.</param>
    public KurrentDBQueueAdapter(string name, KurrentDBOptions options, KurrentDBReceiverOptionsBase receiverOptions, HashRingBasedPartitionedStreamQueueMapper streamQueueMapper, Func<string, IStreamQueueCheckpointer<string>, ILoggerFactory, IKurrentDBQueueCache> cacheFactory, Func<KurrentDBReceiverMonitorDimensions, ILoggerFactory, IQueueAdapterReceiverMonitor> receiverMonitorFactory, Func<KurrentDBReceiverSettings, string, ILogger, IKurrentDBReceiver> receiverFactory, IKurrentDBDataAdapter dataAdapter, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IEnvironmentStatisticsProvider? environmentStatisticsProvider)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(receiverOptions, nameof(receiverOptions));
        ArgumentNullException.ThrowIfNull(streamQueueMapper, nameof(streamQueueMapper));
        ArgumentNullException.ThrowIfNull(cacheFactory, nameof(cacheFactory));
        ArgumentNullException.ThrowIfNull(receiverMonitorFactory, nameof(receiverMonitorFactory));
        ArgumentNullException.ThrowIfNull(receiverFactory, nameof(receiverFactory));
        ArgumentNullException.ThrowIfNull(dataAdapter, nameof(dataAdapter));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        Name = name;
        _options = options;
        _receiverOptions = receiverOptions;
        _streamQueueMapper = streamQueueMapper;
        _cacheFactory = cacheFactory;
        _receiverMonitorFactory = receiverMonitorFactory;
        _receiverFactory = receiverFactory;
        _dataAdapter = dataAdapter;
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _environmentStatisticsProvider = environmentStatisticsProvider;
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
    public StreamProviderDirection Direction { get; protected set; } = StreamProviderDirection.ReadWrite;

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
    public Task QueueMessageBatchAsync<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken token, Dictionary<string, object> requestContext)
    {
        var records = _dataAdapter.ToQueueMessage(streamId, events, token, requestContext);
        if (records.Length == 0)
        {
            return Task.CompletedTask;
        }
        var queueId = _streamQueueMapper.GetQueueForStream(streamId);
        var producer = _producers.GetOrAdd(queueId, static (qid, instance) => instance.MakeProducer(qid), this);
        return producer.AppendAsync(records);
    }

    private KurrentDBProducer MakeProducer(QueueId queueId)
    {
        var producerSettings = new KurrentProducerSettings
                               {
                                   Options = _options,
                                   QueueName = _streamQueueMapper.QueueToPartition(queueId)
                               };
        // The client is a keyed singleton shared by every queue of this connection.
        var client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_options.ConnectionName);
        var producer = new KurrentDBProducer(client, producerSettings, _loggerFactory.CreateLogger<KurrentDBProducer>());
        producer.Init();
        return producer;
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

    private KurrentDBQueueAdapterReceiver GetOrCreateReceiver(QueueId queueId)
    {
        return _receivers.GetOrAdd(queueId, static (qid, instance) => instance.MakeReceiver(qid), this);
    }

    private KurrentDBQueueAdapterReceiver MakeReceiver(QueueId queueId)
    {
        var clusterOptions = _serviceProvider.GetProviderClusterOptions(Name);
        var receiverSettings = new KurrentDBReceiverSettings
                               {
                                   Options = _options,
                                   ReceiverOptions = _receiverOptions,
                                   ConsumerGroup = $"{clusterOptions.Value.ServiceId}-{Name}",
                                   QueueName = _streamQueueMapper.QueueToPartition(queueId)
                               };
        var receiverMonitorDimensions = new KurrentDBReceiverMonitorDimensions
                                        {
                                            QueueName = receiverSettings.QueueName,
                                            Name = receiverSettings.Options.Name
                                        };
        // Should only need checkpointer on silo side, so move its init logic when it is used.
        if (_checkpointerFactory == null)
        {
            _checkpointerFactory = _serviceProvider.GetRequiredKeyedService<IStreamQueueCheckpointerFactory>(Name);
        }
        return new KurrentDBQueueAdapterReceiver(receiverSettings, _cacheFactory, _checkpointerFactory.Create, _loggerFactory, _receiverMonitorFactory(receiverMonitorDimensions, _loggerFactory), _serviceProvider.GetRequiredService<IOptions<LoadSheddingOptions>>().Value, _environmentStatisticsProvider, _receiverFactory);
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
