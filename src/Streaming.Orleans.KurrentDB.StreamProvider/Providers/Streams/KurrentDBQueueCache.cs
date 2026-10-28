using System.Diagnostics.CodeAnalysis;
using Continuum.Streaming.Orleans;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     KurrentDB queue cache.
/// </summary>
public class KurrentDBQueueCache : IKurrentDBQueueCache
{
    private readonly int _defaultMaxAddCount;
    private readonly IObjectPool<FixedSizeBuffer> _bufferPool;
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly IEvictionStrategy _evictionStrategy;
    private readonly KurrentDBCursorTracker _cursorTracker;
    private ulong _newestCommitPosition;
    private readonly ILogger _logger;
    private readonly AggregatedCachePressureMonitor _cachePressureMonitor;
    private readonly ICacheMonitor _cacheMonitor;
    private FixedSizeBuffer? _buffer;
    private int _disposed;

    private readonly PooledQueueCache _cache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueCache" /> class.
    /// </summary>
    /// <param name="queue">Queue this instance is caching.</param>
    /// <param name="defaultMaxAddCount">Default max number of items that can be added to the cache between purge calls.</param>
    /// <param name="bufferPool">raw data block pool.</param>
    /// <param name="dataAdapter">Adapts EventRecord to cached.</param>
    /// <param name="evictionStrategy">Eviction strategy manage purge related events</param>
    /// <param name="cursorTracker">Tracks the cursors reading this cache so purging never passes one.</param>
    /// <param name="logger"></param>
    /// <param name="cacheMonitor"></param>
    /// <param name="cacheMonitorWriteInterval"></param>
    /// <param name="purgeMetadataInterval"></param>
    public KurrentDBQueueCache(string queue,
                                int defaultMaxAddCount,
                                IObjectPool<FixedSizeBuffer> bufferPool,
                                IKurrentDBDataAdapter dataAdapter,
                                IEvictionStrategy evictionStrategy,
                                KurrentDBCursorTracker cursorTracker,
                                ILogger logger,
                                ICacheMonitor cacheMonitor,
                                TimeSpan? cacheMonitorWriteInterval,
                                TimeSpan? purgeMetadataInterval)
    {
        Queue = queue;
        _defaultMaxAddCount = defaultMaxAddCount;
        _bufferPool = bufferPool;
        _dataAdapter = dataAdapter;
        _cursorTracker = cursorTracker;
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

    /// <summary>
    ///     Handles cache purge signals
    /// </summary>
    /// <param name="lastItemPurged"></param>
    /// <param name="newestItem"></param>
    private void OnPurged(CachedMessage? lastItemPurged, CachedMessage? newestItem)
    {
        if (_logger.IsEnabled(LogLevel.Debug) && lastItemPurged.HasValue && newestItem.HasValue)
        {
            _logger.CachePeriod(LogFormatter.PrintDate(lastItemPurged.Value.EnqueueTimeUtc),
                             LogFormatter.PrintDate(newestItem.Value.EnqueueTimeUtc),
                             LogFormatter.PrintDate(lastItemPurged.Value.DequeueTimeUtc),
                             LogFormatter.PrintDate(newestItem.Value.DequeueTimeUtc));
        }
        if (lastItemPurged.HasValue)
        {
            // Purge is the earliest point where an event is provably behind every cursor, so it is the first safe
            // place to advance a checkpoint. The receiver owns that checkpoint, so it is told that these events are
            // done with.
            var commitPosition = _dataAdapter.GetCommitPosition(lastItemPurged.Value);
            OnCheckpointablePosition?.Invoke(commitPosition);
        }
    }

    /// <summary>
    ///     Raised with the <c>$all</c> commit position of the newest event that has been purged, and is therefore
    ///     behind every active cursor.
    /// </summary>
    public Action<ulong>? OnCheckpointablePosition { get; set; }

    #region IKurrentDBQueueCache Implementation

    /// <summary>
    ///     Add a list of KurrentDB EventRecord to the cache.
    /// </summary>
    /// <param name="queueMessages"></param>
    /// <param name="dequeueTimeUtc"></param>
    /// <returns></returns>
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
                // Same as an oversized event: failing the batch would lose every other event in it.
                _logger.InvalidStreamNamespace(queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.StreamNamespace);
                continue;
            }
            catch (InvalidOrleansStreamIdException ex)
            {
                // Same as an invalid namespace: failing the batch would lose every other event in it.
                _logger.InvalidOrleansStreamId(ex, queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.OrleansStreamId);
                continue;
            }
            CachedMessage cachedMessage;
            try
            {
                cachedMessage = _dataAdapter.FromQueueMessage(streamPosition, queueMessage, dequeueTimeUtc, GetSegment);
            }
            catch (EventTooLargeForCacheException ex)
            {
                // The event has already left the receiver, so failing the batch would lose every other event in it.
                // Skipping only this event keeps the rest; the checkpoint moves past it with the next purge.
                _logger.EventTooLargeForCache(queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.Size, ex.BlockSize);
                continue;
            }
            cachedMessages.Add(cachedMessage);
            streamPositions.Add(streamPosition);
        }
        _cache.Add(cachedMessages, dequeueTimeUtc);
        if (cachedMessages.Count > 0)
        {
            var previousNewest = _newestCommitPosition;
            WakeCaughtUpCursors(cachedMessages, streamPositions, previousNewest);
            _newestCommitPosition = _dataAdapter.GetCommitPosition(cachedMessages[^1]);
        }
        return streamPositions;
    }

    private void WakeCaughtUpCursors(List<CachedMessage> cachedMessages, List<StreamPosition> streamPositions, ulong previousNewest)
    {
        HashSet<StreamId>? woken = null;
        for (var i = 0; i < cachedMessages.Count; i++)
        {
            var streamId = streamPositions[i].StreamId;
            if (woken is not null && woken.Contains(streamId))
            {
                continue;
            }
            // The cursor had read every earlier event of its stream, so holding the watermark at the newest position
            // before this batch keeps this and every later event of the stream until it reads them.
            var entries = _cursorTracker.Wake(streamId, previousNewest);
            if (entries.Count == 0)
            {
                continue;
            }
            (woken ??= []).Add(streamId);
            var cachedMessage = cachedMessages[i];
            var token = _dataAdapter.GetSequenceToken(ref cachedMessage);
            foreach (var entry in entries)
            {
                // The old cache cursor may point into blocks purged while it was caught up, so it is replaced with one
                // positioned at the first new event, which was just added and is still cached.
                var result = _cache.TryGetCursor(streamId, token);
                if (result.CacheMiss is { } cacheMiss)
                {
                    throw cacheMiss.ToException();
                }
                entry.CacheCursor = result.Cursor!;
            }
        }
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

    /// <summary>
    ///     Get a cursor into the cache to read events from a stream.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="sequenceToken"></param>
    /// <returns></returns>
    public object GetCursor(StreamId streamId, StreamSequenceToken? sequenceToken)
    {
        var result = _cache.TryGetCursor(streamId, sequenceToken);
        if (result.CacheMiss is { } cacheMiss)
        {
            throw cacheMiss.ToException();
        }
        return _cursorTracker.Register(streamId, result.Cursor!);
    }

    /// <inheritdoc />
    public object GetCursorAtPosition(StreamId streamId, StreamSubscriptionStartPosition startPosition)
    {
        var result = _cache.TryGetCursorAtPosition(streamId, startPosition);
        if (result.CacheMiss is { } cacheMiss)
        {
            throw cacheMiss.ToException();
        }
        return _cursorTracker.Register(streamId, result.Cursor!);
    }

    /// <summary>
    ///     Stop tracking a cursor that is no longer reading, so it no longer holds back purging.
    /// </summary>
    /// <param name="cursorObj"></param>
    public void ReleaseCursor(object cursorObj)
    {
        _cursorTracker.Unregister((KurrentDBCursorEntry)cursorObj);
    }

    /// <inheritdoc />
    public void RecordDelivered(object cursorObj, ulong commitPosition)
    {
        _cursorTracker.RecordProgress((KurrentDBCursorEntry)cursorObj, commitPosition);
    }

    /// <inheritdoc />
    public void RecordCaughtUp(object cursorObj)
    {
        // The cursor has read every cached event of its stream, so it stops holding back purging. Adding a later event
        // of the stream wakes it before any purge can run, so that event is never purged unread.
        _cursorTracker.MarkCaughtUp((KurrentDBCursorEntry)cursorObj);
    }

    /// <summary>
    ///     Try to get the next message in the cache for the provided cursor.
    /// </summary>
    /// <param name="cursorObj"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    public bool TryGetNextMessage(object cursorObj, [MaybeNullWhen(false)] out IBatchContainer container)
    {
        var result = _cache.TryGetNextMessageWithResult(((KurrentDBCursorEntry)cursorObj).CacheCursor, out container);
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
        // pressure is the ratio of the distance from the front of the cache to the
        cachePressureContribution = distanceFromNewestMessage / cacheSize;
        return true;
    }

    /// <summary>
    ///     Add cache pressure monitor to the cache's back pressure algorithm
    /// </summary>
    /// <param name="monitor"></param>
    public void AddCachePressureMonitor(ICachePressureMonitor monitor)
    {
        monitor.CacheMonitor = _cacheMonitor;
        _cachePressureMonitor.AddCachePressureMonitor(monitor);
    }

    /// <inheritdoc />
    public void SignalPurge()
    {
        _cursorTracker.BeginPurge();
        try
        {
            _evictionStrategy.PerformPurge(DateTime.UtcNow);
        }
        finally
        {
            _cursorTracker.EndPurge();
        }
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

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <filterpriority>2</filterpriority>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }
        _evictionStrategy.OnPurged = null;
        // The queue is leaving this silo, so the blocks it still holds go back to the shared pool rather than being
        // stranded, where they would also count as in use and stop the pool from trimming.
        (_evictionStrategy as KurrentDBEvictionStrategy)?.ReleaseBlocks();
        _buffer = null;
        (_bufferPool as KurrentDBQueueCacheBufferPool)?.UnregisterCache();
    }

    #endregion

}

internal static partial class KurrentDBQueueCacheLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "CachePeriod: EnqueueTimeUtc: {OldestEnqueueTimeUtc} to {NewestEnqueueTimeUtc}, DequeueTimeUtc: {OldestDequeueTimeUtc} to {NewestDequeueTimeUtc}")] public static partial void CachePeriod(this ILogger logger, string oldestEnqueueTimeUtc, string newestEnqueueTimeUtc, string oldestDequeueTimeUtc, string newestDequeueTimeUtc);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its cached size of {Size} bytes exceeds the cache block size of {BlockSize} bytes.")] public static partial void EventTooLargeForCache(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, int size, int blockSize);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its stream namespace '{StreamNamespace}' contains the reserved separator ':'.")] public static partial void InvalidStreamNamespace(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string streamNamespace);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its metadata Orleans stream id '{OrleansStreamId}' is not a valid stream id.")] public static partial void InvalidOrleansStreamId(this ILogger logger, Exception exception, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string orleansStreamId);
}
