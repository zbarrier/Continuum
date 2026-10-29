using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;
using Orleans.Statistics;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Adapter factory. This should create an adapter from the stream provider configuration.
/// </summary>
public class KurrentDBQueueAdapterFactory : IQueueAdapterFactory, IDisposable
{
    private readonly string _name;
    private readonly KurrentDBOptions _options;
    private readonly IKurrentDBReceiverFactory _receiverFactory;
    private readonly StreamCacheEvictionOptions _cacheEvictionOptions;
    private readonly StreamStatisticOptions _statisticOptions;
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEnvironmentStatisticsProvider? _environmentStatisticsProvider;

    private HashRingBasedPartitionedStreamQueueMapper? _streamQueueMapper;
    private KurrentDBQueueAdapter? _queueAdapter;
    private bool _ownsDataAdapter;

    /// <summary>
    ///     Creates an instance of the <see cref="KurrentDBQueueAdapterFactory" /> using the provided IServiceProvider and name.
    /// </summary>
    /// <param name="serviceProvider">The IServiceProvider used for dependency injection.</param>
    /// <param name="name">The name used to retrieve options and services from the IServiceProvider.</param>
    /// <returns>An instance of the <see cref="KurrentDBQueueAdapterFactory" />.</returns>
    public static KurrentDBQueueAdapterFactory Create(IServiceProvider serviceProvider, string name)
    {
        var options = serviceProvider.GetOptionsByName<KurrentDBOptions>(name);
        // The subscription strategy is selected through the stream configurator, which registers the matching
        // receiver factory as a keyed service. Only that strategy's options are resolved and validated.
        var receiverFactory = serviceProvider.GetRequiredKeyedService<IKurrentDBReceiverFactory>(name);
        var cachePressureOptions = serviceProvider.GetOptionsByName<KurrentDBStreamCachePressureOptions>(name);
        var cacheEvictionOptions = serviceProvider.GetOptionsByName<StreamCacheEvictionOptions>(name);
        var statisticOptions = serviceProvider.GetOptionsByName<StreamStatisticOptions>(name);
        var dataAdapter = serviceProvider.GetKeyedService<IKurrentDBDataAdapter>(name) ?? serviceProvider.GetService<IKurrentDBDataAdapter>();
        var ownsDataAdapter = dataAdapter is null;
        dataAdapter ??= CreateDefaultDataAdapter(serviceProvider, name);
        var factory = ActivatorUtilities.CreateInstance<KurrentDBQueueAdapterFactory>(serviceProvider, name, options, receiverFactory, cachePressureOptions, cacheEvictionOptions, statisticOptions, dataAdapter);
        // An adapter resolved from the container is disposed by the container; only the default one is owned here.
        factory._ownsDataAdapter = ownsDataAdapter;
        return factory;
    }

    /// <summary>
    ///     Creates the default data adapter using the options configured for this stream provider.
    /// </summary>
    /// <remarks>
    ///     The adapter options are named per stream provider, so they have to be resolved here rather than injected;
    ///     they carry the serializer, type mapper and stream id mapping used to read events written by the event
    ///     sourcing storage.
    /// </remarks>
    private static IKurrentDBDataAdapter CreateDefaultDataAdapter(IServiceProvider serviceProvider, string name)
    {
        var adapterOptions = serviceProvider.GetOptionsByName<KurrentDBDataAdapterOptions>(name);
        return ActivatorUtilities.CreateInstance<KurrentDBQueueDataAdapter>(serviceProvider, adapterOptions);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="KurrentDBQueueAdapterFactory" />.
    /// </summary>
    /// <param name="name">Name of the adapter. Primarily for logging purposes.</param>
    /// <param name="options">The options for connecting to KurrentDB.</param>
    /// <param name="receiverFactory">The factory that creates receivers for the selected subscription strategy.</param>
    /// <param name="cachePressureOptions">The options for configuring the stream cache pressure.</param>
    /// <param name="cacheEvictionOptions">The options for configuring stream cache eviction.</param>
    /// <param name="statisticOptions">The options for configuring statistics collection.</param>
    /// <param name="dataAdapter">The adapter for converting between KurrentDB's data and Orleans' data.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="environmentStatisticsProvider">The environment statistics provider.</param>
    public KurrentDBQueueAdapterFactory(string name, KurrentDBOptions options, IKurrentDBReceiverFactory receiverFactory, KurrentDBStreamCachePressureOptions cachePressureOptions, StreamCacheEvictionOptions cacheEvictionOptions, StreamStatisticOptions statisticOptions, IKurrentDBDataAdapter dataAdapter, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IEnvironmentStatisticsProvider? environmentStatisticsProvider)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(receiverFactory, nameof(receiverFactory));
        ArgumentNullException.ThrowIfNull(cachePressureOptions, nameof(cachePressureOptions));
        ArgumentNullException.ThrowIfNull(cacheEvictionOptions, nameof(cacheEvictionOptions));
        ArgumentNullException.ThrowIfNull(statisticOptions, nameof(statisticOptions));
        ArgumentNullException.ThrowIfNull(dataAdapter, nameof(dataAdapter));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        _name = name;
        _options = options;
        _receiverFactory = receiverFactory;
        _cacheEvictionOptions = cacheEvictionOptions;
        _statisticOptions = statisticOptions;
        _dataAdapter = dataAdapter;
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        _environmentStatisticsProvider = environmentStatisticsProvider;
        CacheFactory = CreateCacheFactory(cachePressureOptions).CreateCache;
        StreamFailureHandlerFactory = _ => Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
        QueueMapperFactory = queues => new HashRingBasedPartitionedStreamQueueMapper(queues, name);
        ReceiverMonitorFactory = (dimensions, _) => new DefaultKurrentDBReceiverMonitor(dimensions, serviceProvider.GetRequiredService<OrleansInstruments>());
        ReceiverFactory = _receiverFactory.Create;
    }

    /// <summary>
    ///     Creates a message cache for a KurrentDB queue.
    /// </summary>
    protected Func<string, ILoggerFactory, IKurrentDBQueueCache> CacheFactory { get; set; }

    /// <summary>
    ///     Creates a failure handler for a queue.
    /// </summary>
    protected Func<string, Task<IStreamFailureHandler>> StreamFailureHandlerFactory { get; set; }

    /// <summary>
    ///     Create a queue mapper to map KurrentDB streams to queues.
    /// </summary>
    protected Func<string[], HashRingBasedPartitionedStreamQueueMapper> QueueMapperFactory { get; set; }

    /// <summary>
    ///     Create a receiver monitor to report performance metrics.
    ///     Factory function should return an IKurrentDBReceiverMonitor.
    /// </summary>
    protected Func<KurrentDBReceiverMonitorDimensions, ILoggerFactory, IQueueAdapterReceiverMonitor> ReceiverMonitorFactory { get; set; }

    /// <summary>
    ///     Factory to create a IKurrentDBReceiver.
    ///     for testing purpose, used in KurrentDBGeneratorStreamProvider
    /// </summary>
    protected Func<KurrentDBReceiverSettings, ILogger, IKurrentDBReceiver> ReceiverFactory { get; set; }

    /// <summary>
    ///     Create a IKurrentDBQueueCacheFactory. It will create a KurrentDBQueueCacheFactory by default.
    ///     User can override this function to return their own implementation of IKurrentDBQueueCacheFactory,
    ///     and other customization of IKurrentDBQueueCacheFactory if they may.
    /// </summary>
    /// <returns></returns>
    protected virtual IKurrentDBQueueCacheFactory CreateCacheFactory(KurrentDBStreamCachePressureOptions cachePressureOptions)
    {
        var sharedDimensions = new KurrentDBMonitorAggregationDimensions(_options.Name);
        var bufferPoolOptions = _serviceProvider.GetOptionsByName<KurrentDBStreamCacheBufferPoolOptions>(_name);
        return new KurrentDBQueueCacheFactory(_receiverFactory, cachePressureOptions, bufferPoolOptions, _cacheEvictionOptions, _statisticOptions, _dataAdapter, sharedDimensions, _serviceProvider.GetRequiredService<OrleansInstruments>(), _serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System);
    }

    /// <summary>
    ///     Get queue names (aka stream names) from KurrentDB.
    /// </summary>
    /// <returns></returns>
    protected virtual string[] GetQueueNames()
    {
        return _options.Queues.ToArray();
    }

    #region IQueueAdapterFactory Implementation

    /// <summary>
    ///     Creates a queue adapter.
    /// </summary>
    /// <returns>The queue adapter</returns>
    public Task<IQueueAdapter> CreateAdapter()
    {
        return Task.FromResult<IQueueAdapter>(GetOrCreateAdapter());
    }

    private KurrentDBQueueAdapter GetOrCreateAdapter()
    {
        if (_queueAdapter == null)
        {
            var streamQueueMapper = GetOrCreateStreamQueueMapper();
            _queueAdapter = new KurrentDBQueueAdapter(_name, _options, _receiverFactory, streamQueueMapper, CacheFactory, ReceiverMonitorFactory, ReceiverFactory, _dataAdapter, _serviceProvider, _loggerFactory, _environmentStatisticsProvider);
        }
        return _queueAdapter;
    }

    /// <summary>
    ///     Creates queue message cache adapter.
    /// </summary>
    /// <returns>The queue adapter cache.</returns>
    public IQueueAdapterCache GetQueueAdapterCache()
    {
        return GetOrCreateAdapter();
    }

    /// <summary>
    ///     Creates a queue mapper.
    /// </summary>
    /// <returns>The queue mapper.</returns>
    public IStreamQueueMapper GetStreamQueueMapper()
    {
        return GetOrCreateStreamQueueMapper();
    }

    private HashRingBasedPartitionedStreamQueueMapper GetOrCreateStreamQueueMapper()
    {
        if (_streamQueueMapper == null)
        {
            _streamQueueMapper = QueueMapperFactory.Invoke(GetQueueNames());
        }
        return _streamQueueMapper;
    }

    /// <summary>
    ///     Acquire delivery failure handler for a queue
    /// </summary>
    /// <param name="queueId">The queue identifier.</param>
    /// <returns>The stream failure handler.</returns>
    public Task<IStreamFailureHandler> GetDeliveryFailureHandler(QueueId queueId)
    {
        if (_streamQueueMapper != null)
        {
            var queue = _streamQueueMapper.QueueToPartition(queueId);
            return StreamFailureHandlerFactory.Invoke(queue);
        }
        return Task.FromResult<IStreamFailureHandler>(new NoOpStreamDeliveryFailureHandler());
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the data adapter when this factory created it.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> when called from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && _ownsDataAdapter && _dataAdapter is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
