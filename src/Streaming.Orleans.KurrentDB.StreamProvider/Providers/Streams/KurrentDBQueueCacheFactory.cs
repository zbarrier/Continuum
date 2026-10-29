using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;
using Orleans.Runtime;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Factory class to configure and create IKurrentDBQueueCache
/// </summary>
public class KurrentDBQueueCacheFactory : IKurrentDBQueueCacheFactory
{
    private readonly KurrentDBStreamCachePressureOptions _pressureOptions;
    private readonly KurrentDBStreamCacheBufferPoolOptions _bufferPoolOptions;
    private readonly TimeProvider _timeProvider;
    private readonly StreamCacheEvictionOptions _evictionOptions;
    private readonly StreamStatisticOptions _statisticOptions;
    private readonly IKurrentDBDataAdapter _dataAdater;
    private readonly KurrentDBMonitorAggregationDimensions _sharedDimensions;
    private readonly OrleansInstruments _instruments;
    private readonly TimePurgePredicate _timePurge;
    private readonly IKurrentDBReceiverFactory _strategy;

    private IObjectPool<FixedSizeBuffer>? _bufferPool;
    private string? _bufferPoolId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueCacheFactory" /> class.
    /// </summary>
    public KurrentDBQueueCacheFactory(IKurrentDBReceiverFactory strategy,
                                       KurrentDBStreamCachePressureOptions pressureOptions,
                                       KurrentDBStreamCacheBufferPoolOptions bufferPoolOptions,
                                       StreamCacheEvictionOptions evictionOptions,
                                       StreamStatisticOptions statisticOptions,
                                       IKurrentDBDataAdapter dataAdater,
                                       KurrentDBMonitorAggregationDimensions sharedDimensions,
                                       OrleansInstruments instruments,
                                       TimeProvider timeProvider,
                                       Func<KurrentDBCacheMonitorDimensions, ILoggerFactory, ICacheMonitor>? cacheMonitorFactory = null,
                                       Func<KurrentDBBlockPoolMonitorDimensions, ILoggerFactory, IBlockPoolMonitor>? blockPoolMonitorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(strategy, nameof(strategy));
        ArgumentNullException.ThrowIfNull(pressureOptions, nameof(pressureOptions));
        ArgumentNullException.ThrowIfNull(bufferPoolOptions, nameof(bufferPoolOptions));
        ArgumentNullException.ThrowIfNull(evictionOptions, nameof(evictionOptions));
        ArgumentNullException.ThrowIfNull(statisticOptions, nameof(statisticOptions));
        ArgumentNullException.ThrowIfNull(dataAdater, nameof(dataAdater));
        ArgumentNullException.ThrowIfNull(sharedDimensions, nameof(sharedDimensions));
        ArgumentNullException.ThrowIfNull(instruments, nameof(instruments));
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        _pressureOptions = pressureOptions;
        _bufferPoolOptions = bufferPoolOptions;
        _timeProvider = timeProvider;
        _evictionOptions = evictionOptions;
        _statisticOptions = statisticOptions;
        _dataAdater = dataAdater;
        _sharedDimensions = sharedDimensions;
        _instruments = instruments;
        _strategy = strategy;
        _timePurge = new TimePurgePredicate(evictionOptions.DataMinTimeInCache, evictionOptions.DataMaxAgeInCache);
        CacheMonitorFactory = cacheMonitorFactory ?? ((dimensions, _) => new DefaultKurrentDBCacheMonitor(dimensions, _instruments));
        BlockPoolMonitorFactory = blockPoolMonitorFactory ?? ((dimensions, _) => new DefaultKurrentDBBlockPoolMonitor(dimensions, _instruments));
    }

    /// <summary>
    ///     Create a cache monitor to report performance metrics.
    ///     Factory function should return an ICacheMonitor.
    /// </summary>
    public Func<KurrentDBCacheMonitorDimensions, ILoggerFactory, ICacheMonitor> CacheMonitorFactory { get; set; }

    /// <summary>
    ///     Create a block pool monitor to report performance metrics.
    ///     Factory function should return an IObjectPoolMonitor.
    /// </summary>
    public Func<KurrentDBBlockPoolMonitorDimensions, ILoggerFactory, IBlockPoolMonitor> BlockPoolMonitorFactory { get; set; }

    /// <summary>
    ///     Creates the selected strategy's queue cache, configured using the CreateBufferPool function
    ///     and AddCachePressureMonitors function.
    /// </summary>
    /// <returns></returns>
    public IKurrentDBQueueCache CreateCache(string queue, ILoggerFactory loggerFactory)
    {
        var blockPool = CreateBufferPool(_statisticOptions, loggerFactory, _sharedDimensions, out var blockPoolId);
        var cache = CreateCache(queue, _dataAdater, _statisticOptions, _evictionOptions, loggerFactory, blockPool, blockPoolId, _timePurge, _sharedDimensions);
        AddCachePressureMonitors(cache, _pressureOptions, loggerFactory.CreateLogger($"{cache.GetType().FullName}.{_sharedDimensions.Name}.{queue}"));
        return cache;
    }

    /// <summary>
    ///     Function used to configure BufferPool for the queue caches.
    ///     User can override this function to provide more customization on BufferPool creation
    /// </summary>
    protected virtual IObjectPool<FixedSizeBuffer> CreateBufferPool(StreamStatisticOptions statisticOptions, ILoggerFactory loggerFactory, KurrentDBMonitorAggregationDimensions sharedDimensions, out string blockPoolId)
    {
        if (_bufferPool == null)
        {
            // KurrentDB accepts events up to 1 MB of data and metadata. The cache stores strings, including metadata,
            // as UTF-16, so blocks are 2 MB to leave room for that and the other encoded fields.
            var bufferSize = 1 << 21;
            _bufferPoolId = $"BlockPool-{Guid.NewGuid()}-BlockSize-{bufferSize}";
            var monitorDimensions = new KurrentDBBlockPoolMonitorDimensions(sharedDimensions, _bufferPoolId);
            var objectPoolMonitor = new ObjectPoolMonitorBridge(BlockPoolMonitorFactory(monitorDimensions, loggerFactory), bufferSize);
            _bufferPool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(bufferSize), _bufferPoolOptions, _timeProvider, objectPoolMonitor, statisticOptions.StatisticMonitorWriteInterval);
        }
        blockPoolId = _bufferPoolId!;
        return _bufferPool;
    }

    /// <summary>
    ///     Function used to configure cache pressure monitors for the queue caches.
    ///     User can override this function to provide more customization on cache pressure monitors
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="providerOptions"></param>
    /// <param name="cacheLogger"></param>
    protected virtual void AddCachePressureMonitors(IKurrentDBQueueCache cache, KurrentDBStreamCachePressureOptions providerOptions, ILogger cacheLogger)
    {
        if (providerOptions.AveragingCachePressureMonitorFlowControlThreshold.HasValue)
        {
            var avgMonitor = new AveragingCachePressureMonitor(providerOptions.AveragingCachePressureMonitorFlowControlThreshold.Value, cacheLogger);
            cache.AddCachePressureMonitor(avgMonitor);
        }
        if (!providerOptions.SlowConsumingMonitorPressureWindowSize.HasValue && !providerOptions.SlowConsumingMonitorFlowControlThreshold.HasValue)
        {
            return;
        }
        var slowConsumeMonitor = new SlowConsumingPressureMonitor(cacheLogger);
        if (providerOptions.SlowConsumingMonitorFlowControlThreshold.HasValue)
        {
            slowConsumeMonitor.FlowControlThreshold = providerOptions.SlowConsumingMonitorFlowControlThreshold.Value;
        }
        if (providerOptions.SlowConsumingMonitorPressureWindowSize.HasValue)
        {
            slowConsumeMonitor.PressureWindowSize = providerOptions.SlowConsumingMonitorPressureWindowSize.Value;
        }
        cache.AddCachePressureMonitor(slowConsumeMonitor);
    }

    /// <summary>
    ///     Default function to be called to create the strategy's queue cache in IKurrentDBQueueCacheFactory.CreateCache method. User can
    ///     override this method to add more customization.
    /// </summary>
    protected virtual IKurrentDBQueueCache CreateCache(string queue,
                                                        IKurrentDBDataAdapter dataAdatper,
                                                        StreamStatisticOptions statisticOptions,
                                                        StreamCacheEvictionOptions cacheEvictionOptions,
                                                        ILoggerFactory loggerFactory,
                                                        IObjectPool<FixedSizeBuffer> bufferPool,
                                                        string blockPoolId,
                                                        TimePurgePredicate timePurge,
                                                        KurrentDBMonitorAggregationDimensions sharedDimensions)
    {
        var cacheMonitorDimensions = new KurrentDBCacheMonitorDimensions(sharedDimensions, queue, blockPoolId);
        var cacheMonitor = CacheMonitorFactory(cacheMonitorDimensions, loggerFactory);
        return _strategy.CreateQueueCache(new KurrentDBQueueCacheContext
        {
            Queue = queue,
            DataAdapter = dataAdatper,
            BufferPool = bufferPool,
            TimePurge = timePurge,
            CacheMonitor = cacheMonitor,
            LoggerFactory = loggerFactory,
            LoggerNameSuffix = $"{sharedDimensions.Name}.{queue}",
            StatisticMonitorWriteInterval = statisticOptions.StatisticMonitorWriteInterval,
            MetadataMinTimeInCache = cacheEvictionOptions.MetadataMinTimeInCache,
        });
    }

}
