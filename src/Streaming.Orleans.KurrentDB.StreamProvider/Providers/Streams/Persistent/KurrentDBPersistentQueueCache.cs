using System.Diagnostics.CodeAnalysis;

using Continuum.Streaming.Orleans;

using Microsoft.Extensions.Logging;

using Orleans.Providers.Streams.Common;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     KurrentDB queue cache for persistent subscriptions.
/// </summary>
/// <remarks>
///     Persistent subscriptions acknowledge events as they are received and keep their checkpoint on the server, and
///     may redeliver or reorder events. The cache therefore purges on age alone, does not track cursors, and never
///     raises a checkpoint.
/// </remarks>
public class KurrentDBPersistentQueueCache : IKurrentDBQueueCache
{
    private readonly int _defaultMaxAddCount;
    private readonly IObjectPool<FixedSizeBuffer> _bufferPool;
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly IEvictionStrategy _evictionStrategy;
    private readonly ILogger _logger;
    private readonly AggregatedCachePressureMonitor _cachePressureMonitor;
    private readonly ICacheMonitor _cacheMonitor;
    private FixedSizeBuffer? _buffer;
    private int _disposed;

    private readonly PooledQueueCache _cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBPersistentQueueCache" /> class.
    /// </summary>
    /// <param name="queue">Queue this instance is caching.</param>
    /// <param name="defaultMaxAddCount">Default max number of items that can be added to the cache between purge calls.</param>
    /// <param name="bufferPool">raw data block pool.</param>
    /// <param name="dataAdapter">Adapts EventRecord to cached.</param>
    /// <param name="evictionStrategy">Eviction strategy manage purge related events</param>
    /// <param name="logger"></param>
    /// <param name="cacheMonitor"></param>
    /// <param name="cacheMonitorWriteInterval"></param>
    /// <param name="purgeMetadataInterval"></param>
    public KurrentDBPersistentQueueCache(string queue,
                                          int defaultMaxAddCount,
                                          IObjectPool<FixedSizeBuffer> bufferPool,
                                          IKurrentDBDataAdapter dataAdapter,
                                          IEvictionStrategy evictionStrategy,
                                          ILogger logger,
                                          ICacheMonitor cacheMonitor,
                                          TimeSpan? cacheMonitorWriteInterval,
                                          TimeSpan? purgeMetadataInterval)
    {
        Queue = queue;
        _defaultMaxAddCount = defaultMaxAddCount;
        _bufferPool = bufferPool;
        _dataAdapter = dataAdapter;
        _cache = new PooledQueueCache(dataAdapter, logger, cacheMonitor, cacheMonitorWriteInterval, purgeMetadataInterval);
        _cacheMonitor = cacheMonitor;
        _evictionStrategy = evictionStrategy;
        _evictionStrategy.OnPurged = OnPurged;
        _evictionStrategy.PurgeObservable = _cache;
        _cachePressureMonitor = new AggregatedCachePressureMonitor(logger, cacheMonitor);
        _logger = logger;
        (_bufferPool as KurrentDBQueueCacheBufferPool)?.RegisterCache();
    }

    /// <summary>
    ///     Queue this instance is caching.
    /// </summary>
    public string Queue { get; }

    private void OnPurged(CachedMessage? lastItemPurged, CachedMessage? newestItem)
    {
        if (_logger.IsEnabled(LogLevel.Debug) && lastItemPurged.HasValue && newestItem.HasValue)
        {
            _logger.PersistentCachePeriod(LogFormatter.PrintDate(lastItemPurged.Value.EnqueueTimeUtc),
                                          LogFormatter.PrintDate(newestItem.Value.EnqueueTimeUtc),
                                          LogFormatter.PrintDate(lastItemPurged.Value.DequeueTimeUtc),
                                          LogFormatter.PrintDate(newestItem.Value.DequeueTimeUtc));
        }
    }

    #region IKurrentDBQueueCache Implementation

    /// <inheritdoc />
    public List<StreamPosition> Add(List<StreamedEvent<KurrentDBRawEvent>> queueMessages, DateTime dequeueTimeUtc)
    {
        var streamPositions = new List<StreamPosition>(queueMessages.Count);
        var cachedMessages = new List<CachedMessage>(queueMessages.Count);
        foreach (var queueMessage in queueMessages)
        {
            StreamPosition streamPosition;
            try
            {
                streamPosition = _dataAdapter.GetStreamPosition(queueMessage);
            }
            catch (InvalidStreamNamespaceException ex)
            {
                // Failing the batch would lose every other event in it.
                _logger.PersistentInvalidStreamNamespace(queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.StreamNamespace);
                continue;
            }
            catch (InvalidOrleansStreamIdException ex)
            {
                // Failing the batch would lose every other event in it.
                _logger.PersistentInvalidOrleansStreamId(ex, queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.OrleansStreamId);
                continue;
            }
            CachedMessage cachedMessage;
            try
            {
                cachedMessage = _dataAdapter.FromQueueMessage(streamPosition, queueMessage, dequeueTimeUtc, GetSegment);
            }
            catch (EventTooLargeForCacheException ex)
            {
                // The event has already been acknowledged, so failing the batch would lose every other event in it.
                _logger.PersistentEventTooLargeForCache(queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.Size, ex.BlockSize);
                continue;
            }
            cachedMessages.Add(cachedMessage);
            streamPositions.Add(streamPosition);
        }
        _cache.Add(cachedMessages, dequeueTimeUtc);
        return streamPositions;
    }

    private ArraySegment<byte> GetSegment(int size)
    {
        // get segment from current block
        if (_buffer == null || !_buffer.TryGetSegment(size, out var segment))
        {
            // no block or block full, get new block and try again
            _buffer = _bufferPool.Allocate();
            //call EvictionStrategy's OnBlockAllocated method
            _evictionStrategy.OnBlockAllocated(_buffer);
            // if this fails with clean block, then requested size is too big
            if (!_buffer.TryGetSegment(size, out segment))
            {
                throw new EventTooLargeForCacheException(size, _buffer.SizeInByte);
            }
        }
        return segment;
    }

    /// <inheritdoc />
    public object GetCursor(StreamId streamId, StreamSequenceToken? sequenceToken)
    {
        var result = _cache.TryGetCursor(streamId, sequenceToken);
        if (result.CacheMiss is { } cacheMiss)
        {
            throw cacheMiss.ToException();
        }
        return result.Cursor!;
    }

    /// <inheritdoc />
    public object GetCursorAtPosition(StreamId streamId, StreamSubscriptionStartPosition startPosition)
    {
        var result = _cache.TryGetCursorAtPosition(streamId, startPosition);
        if (result.CacheMiss is { } cacheMiss)
        {
            throw cacheMiss.ToException();
        }
        return result.Cursor!;
    }

    /// <inheritdoc />
    public bool TryGetNextMessage(object cursorObj, [MaybeNullWhen(false)] out IBatchContainer container)
    {
        var result = _cache.TryGetNextMessageWithResult(cursorObj, out container);
        if (result.CacheMiss is { } cacheMiss)
        {
            throw cacheMiss.ToException();
        }
        if (result.Kind != QueueCacheCursorMoveResultKind.Success || container is null)
        {
            return false;
        }
        _cachePressureMonitor.RecordCachePressureContribution(TryCalculateCachePressureContribution(container.SequenceToken, out var cachePressureContribution) ? cachePressureContribution : 0.0);
        return true;
    }

    /// <summary>
    ///     cachePressureContribution should be a double between 0-1, indicating how much danger the item is of being removed from the cache.
    ///     0 indicating  no danger,
    ///     1 indicating removal is imminent.
    /// </summary>
    private bool TryCalculateCachePressureContribution(StreamSequenceToken sequenceToken, out double cachePressureContribution)
    {
        cachePressureContribution = 0;
        // if cache is empty or has few items, don't calculate pressure
        if (_cache.IsEmpty || !_cache.Newest.HasValue || !_cache.Oldest.HasValue || _cache.Newest.Value.SequenceNumber - _cache.Oldest.Value.SequenceNumber < 10 * _defaultMaxAddCount)
        {
            // not enough items in cache.
            return false;
        }
        double cacheSize = _cache.Newest.Value.SequenceNumber - _cache.Oldest.Value.SequenceNumber;
        var distanceFromNewestMessage = _cache.Newest.Value.SequenceNumber - sequenceToken.SequenceNumber;
        // pressure is the ratio of the distance from the front of the cache to the size of the cache
        cachePressureContribution = distanceFromNewestMessage / cacheSize;
        return true;
    }

    /// <inheritdoc />
    public void AddCachePressureMonitor(ICachePressureMonitor monitor)
    {
        monitor.CacheMonitor = _cacheMonitor;
        _cachePressureMonitor.AddCachePressureMonitor(monitor);
    }

    /// <inheritdoc />
    public void SignalPurge()
    {
        _evictionStrategy.PerformPurge(DateTime.UtcNow);
    }

    #endregion

    #region IQueueFlowController Implementation

    /// <summary>
    ///     The limit of the maximum number of items that can be added
    /// </summary>
    public int GetMaxAddCount()
    {
        return _cachePressureMonitor.IsUnderPressure(DateTime.UtcNow) ? 0 : _defaultMaxAddCount;
    }

    #endregion

    #region IDisposable Implementation

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }
        _evictionStrategy.OnPurged = null;
        // The queue is leaving this silo, so the blocks it still holds go back to the shared pool rather than being
        // stranded, where they would also count as in use and stop the pool from trimming.
        (_evictionStrategy as KurrentDBPersistentEvictionStrategy)?.ReleaseBlocks();
        _buffer = null;
        (_bufferPool as KurrentDBQueueCacheBufferPool)?.UnregisterCache();
    }

    #endregion

}

internal static partial class KurrentDBPersistentQueueCacheLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "CachePeriod: EnqueueTimeUtc: {OldestEnqueueTimeUtc} to {NewestEnqueueTimeUtc}, DequeueTimeUtc: {OldestDequeueTimeUtc} to {NewestDequeueTimeUtc}")] public static partial void PersistentCachePeriod(this ILogger logger, string oldestEnqueueTimeUtc, string newestEnqueueTimeUtc, string oldestDequeueTimeUtc, string newestDequeueTimeUtc);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its cached size of {Size} bytes exceeds the cache block size of {BlockSize} bytes.")] public static partial void PersistentEventTooLargeForCache(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, int size, int blockSize);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its stream namespace '{StreamNamespace}' contains the reserved separator ':'.")] public static partial void PersistentInvalidStreamNamespace(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string streamNamespace);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its metadata Orleans stream id '{OrleansStreamId}' is not a valid stream id.")] public static partial void PersistentInvalidOrleansStreamId(this ILogger logger, Exception exception, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string orleansStreamId);
}
