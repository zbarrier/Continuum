using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Providers.Streams.KurrentDB.StatisticMonitors;
using Orleans.Runtime;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Factory class to configure and create IKurrentDBQueueCache
/// </summary>
public class KurrentDBQueueCacheFactory : IKurrentDBQueueCacheFactory
{
    private readonly KurrentDBStreamCachePressureOptions _pressureOptions;
    private readonly StreamCacheEvictionOptions _evictionOptions;
    private readonly StreamStatisticOptions _statisticOptions;
    private readonly IKurrentDBDataAdapter _dataAdater;
    private readonly KurrentDBMonitorAggregationDimensions _sharedDimensions;
    private readonly OrleansInstruments _instruments;
    private readonly TimePurgePredicate _timePurge;

    private IObjectPool<FixedSizeBuffer>? _bufferPool;
    private string? _bufferPoolId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueCacheFactory" /> class.
    /// </summary>
    public KurrentDBQueueCacheFactory(KurrentDBStreamCachePressureOptions pressureOptions,
                                       StreamCacheEvictionOptions evictionOptions,
                                       StreamStatisticOptions statisticOptions,
                                       IKurrentDBDataAdapter dataAdater,
                                       KurrentDBMonitorAggregationDimensions sharedDimensions,
                                       OrleansInstruments instruments,
                                       Func<KurrentDBCacheMonitorDimensions, ILoggerFactory, ICacheMonitor>? cacheMonitorFactory = null,
                                       Func<KurrentDBBlockPoolMonitorDimensions, ILoggerFactory, IBlockPoolMonitor>? blockPoolMonitorFactory = null)
    {
        ArgumentNullException.ThrowIfNull(pressureOptions, nameof(pressureOptions));
        ArgumentNullException.ThrowIfNull(evictionOptions, nameof(evictionOptions));
        ArgumentNullException.ThrowIfNull(statisticOptions, nameof(statisticOptions));
        ArgumentNullException.ThrowIfNull(dataAdater, nameof(dataAdater));
        ArgumentNullException.ThrowIfNull(sharedDimensions, nameof(sharedDimensions));
        ArgumentNullException.ThrowIfNull(instruments, nameof(instruments));
        _pressureOptions = pressureOptions;
        _evictionOptions = evictionOptions;
        _statisticOptions = statisticOptions;
        _dataAdater = dataAdater;
        _sharedDimensions = sharedDimensions;
        _instruments = instruments;
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
    ///     Function which create an KurrentDBQueueCache, which by default will configure the KurrentDBQueueCache using configuration in CreateBufferPool function
    ///     and AddCachePressureMonitors function.
    /// </summary>
    /// <returns></returns>
    public IKurrentDBQueueCache CreateCache(string queue, IStreamQueueCheckpointer<string> checkpointer, ILoggerFactory loggerFactory)
    {
        var blockPool = CreateBufferPool(_statisticOptions, loggerFactory, _sharedDimensions, out var blockPoolId);
        var cache = CreateCache(queue, _dataAdater, _statisticOptions, _evictionOptions, checkpointer, loggerFactory, blockPool, blockPoolId, _timePurge, _sharedDimensions);
        AddCachePressureMonitors(cache, _pressureOptions, loggerFactory.CreateLogger($"{typeof(KurrentDBQueueCache).FullName}.{_sharedDimensions.Name}.{queue}"));
        return cache;
    }

    /// <summary>
    ///     Function used to configure BufferPool for KurrentDBQueueCache.
    ///     User can override this function to provide more customization on BufferPool creation
    /// </summary>
    protected virtual IObjectPool<FixedSizeBuffer> CreateBufferPool(StreamStatisticOptions statisticOptions, ILoggerFactory loggerFactory, KurrentDBMonitorAggregationDimensions sharedDimensions, out string blockPoolId)
    {
        if (_bufferPool == null)
        {
            var bufferSize = 1 << 20;
            _bufferPoolId = $"BlockPool-{new Guid().ToString()}-BlockSize-{bufferSize}";
            var monitorDimensions = new KurrentDBBlockPoolMonitorDimensions(sharedDimensions, _bufferPoolId);
            var objectPoolMonitor = new ObjectPoolMonitorBridge(BlockPoolMonitorFactory(monitorDimensions, loggerFactory), bufferSize);
            _bufferPool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(bufferSize), objectPoolMonitor, statisticOptions.StatisticMonitorWriteInterval);
        }
        blockPoolId = _bufferPoolId!;
        return _bufferPool;
    }

    /// <summary>
    ///     Function used to configure cache pressure monitors for KurrentDBQueueCache.
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
    ///     Default function to be called to create an KurrentDBQueueCache in IKurrentDBQueueCacheFactory.CreateCache method. User can
    ///     override this method to add more customization.
    /// </summary>
    protected virtual IKurrentDBQueueCache CreateCache(string queue,
                                                        IKurrentDBDataAdapter dataAdatper,
                                                        StreamStatisticOptions statisticOptions,
                                                        StreamCacheEvictionOptions cacheEvictionOptions,
                                                        IStreamQueueCheckpointer<string> checkpointer,
                                                        ILoggerFactory loggerFactory,
                                                        IObjectPool<FixedSizeBuffer> bufferPool,
                                                        string blockPoolId,
                                                        TimePurgePredicate timePurge,
                                                        KurrentDBMonitorAggregationDimensions sharedDimensions)
    {
        var cacheMonitorDimensions = new KurrentDBCacheMonitorDimensions(sharedDimensions, queue, blockPoolId);
        var cacheMonitor = CacheMonitorFactory(cacheMonitorDimensions, loggerFactory);
        var logger = loggerFactory.CreateLogger($"{typeof(KurrentDBQueueCache).FullName}.{sharedDimensions.Name}.{queue}");
        var cursorTracker = new KurrentDBCursorTracker();
        var evictionStrategy = new KurrentDBEvictionStrategy(logger, timePurge, cacheMonitor, statisticOptions.StatisticMonitorWriteInterval, dataAdatper, cursorTracker);
        return new KurrentDBQueueCache(queue, KurrentDBQueueAdapterReceiver.MaxMessagesPerRead, bufferPool, dataAdatper, evictionStrategy, checkpointer, cursorTracker, logger, cacheMonitor, statisticOptions.StatisticMonitorWriteInterval, cacheEvictionOptions.MetadataMinTimeInCache);
    }

}
