using System.Diagnostics.CodeAnalysis;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     KurrentDB queue cache.
/// </summary>
public class KurrentDBCatchupQueueCache : IKurrentDBCatchupQueueCache
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
    ///     Initializes a new instance of the <see cref="KurrentDBCatchupQueueCache" /> class.
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
    public KurrentDBCatchupQueueCache(string queue,
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
            OnCheckpointablePosition?.Invoke(_dataAdapter.GetCommitPosition(lastItemPurged.Value));
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
    public List<StreamPosition> Add(IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> queueMessages, DateTime dequeueTimeUtc)
    {
        var streamPositions = new List<StreamPosition>(queueMessages.Count);
        var cachedMessages = new List<CachedMessage>(queueMessages.Count);
        ulong? skippedTail = null;
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
                skippedTail = TrySkip(queueMessage, skippedTail);
                continue;
            }
            catch (InvalidOrleansStreamIdException ex)
            {
                // Same as an invalid namespace: failing the batch would lose every other event in it.
                _logger.InvalidOrleansStreamId(ex, queueMessage.EventId, queueMessage.EventType, queueMessage.StreamName.Value, queueMessage.Cursor.SequenceNumber, ex.OrleansStreamId);
                skippedTail = TrySkip(queueMessage, skippedTail);
                continue;
            }
            if (_pending.Count > 0 && string.Equals(queueMessage.StreamName.Value, KurrentDBStreamedEventConversions.CheckpointStreamName.Value, StringComparison.Ordinal))
            {
                // A checkpoint marker only advances the checkpoint, which must not pass a transaction still being read.
                // The next marker after the transaction completes covers it.
                continue;
            }
            if (_pending.Count > 0 && !BelongsToPending(queueMessage))
            {
                // Events of one append are contiguous in $all, so a different event confirms the pending transaction
                // can never complete.
                skippedTail = HandlePartialTransaction(dequeueTimeUtc, cachedMessages, streamPositions, skippedTail);
            }
            _pending.Add((queueMessage, streamPosition));
            _pendingLastReceivedUtc = dequeueTimeUtc;
            if (IsTransactionComplete())
            {
                skippedTail = CacheTransaction(dequeueTimeUtc, cachedMessages, streamPositions, skippedTail);
            }
        }
        return Commit(cachedMessages, streamPositions, skippedTail, dequeueTimeUtc);
    }

    /// <summary>
    ///     What to do with a transaction KurrentDB has confirmed was only partially received.
    /// </summary>
    public PartialTransactionHandling PartialTransactionHandling { get; set; } = PartialTransactionHandling.Halt;

    /// <inheritdoc />
    public PendingTransaction? PendingTransaction =>
        _pending.Count > 0 && _pending[0].Message.Metadata.TransactionId is { } transactionId
            ? new PendingTransaction(transactionId, (ulong)_pending[^1].Message.Cursor.SequenceNumber, _pendingLastReceivedUtc)
            : null;

    /// <inheritdoc />
    public List<StreamPosition> ReleasePartialTransaction(DateTime utcNow)
    {
        if (_pending.Count == 0)
        {
            return [];
        }
        var streamPositions = new List<StreamPosition>();
        var cachedMessages = new List<CachedMessage>();
        var skippedTail = HandlePartialTransaction(utcNow, cachedMessages, streamPositions, null);
        return Commit(cachedMessages, streamPositions, skippedTail, utcNow);
    }

    private ulong? HandlePartialTransaction(DateTime dequeueTimeUtc, List<CachedMessage> cachedMessages, List<StreamPosition> streamPositions, ulong? skippedTail)
    {
        var first = _pending[0].Message.Metadata;
        var lastPosition = (ulong)_pending[^1].Message.Cursor.SequenceNumber;
        switch (PartialTransactionHandling)
        {
            case PartialTransactionHandling.Deliver:
                _logger.PartialTransactionDelivered(first.TransactionId, _pending.Count, first.TransactionSize, lastPosition);
                break;
            case PartialTransactionHandling.DeliverAndWarn:
                _logger.PartialTransactionDeliveredWithWarning(first.TransactionId, _pending.Count, first.TransactionSize, lastPosition);
                break;
            default:
                // Nothing of the transaction is cached, so no purge, and therefore no checkpoint, can pass it. The
                // pending events are kept so the transaction stays visible while halted; they are read again on restart.
                _logger.PartialTransactionHalted(first.TransactionId, _pending.Count, first.TransactionSize, lastPosition);
                throw new KurrentDBPartialTransactionException(first.TransactionId, lastPosition, _pending.Count, first.TransactionSize);
        }
        return CacheTransaction(dequeueTimeUtc, cachedMessages, streamPositions, skippedTail);
    }

    private DateTime _pendingLastReceivedUtc;

    private List<StreamPosition> Commit(List<CachedMessage> cachedMessages, List<StreamPosition> streamPositions, ulong? skippedTail, DateTime dequeueTimeUtc)
    {
        _cache.Add(cachedMessages, dequeueTimeUtc);
        if (cachedMessages.Count > 0)
        {
            var previousNewest = _newestCommitPosition;
            // The batch is already cached, so the newest position moves first and the positions are always returned.
            _newestCommitPosition = _dataAdapter.GetCommitPosition(cachedMessages[^1]);
            WakeCaughtUpCursors(cachedMessages, streamPositions, previousNewest);
        }
        else if (skippedTail.HasValue && _cache.IsEmpty && _pending.Count == 0)
        {
            // Nothing cached is in front of the skipped events, so no purge will ever report them.
            OnCheckpointablePosition?.Invoke(skippedTail.Value);
        }
        return streamPositions;
    }

    // A transaction is only cached once every event of it has been read, so it is always delivered whole. Its events
    // are held here between reads; they have not reached the cache, so neither purge nor checkpoint can pass them.
    private readonly List<(StreamedEvent<KurrentDBRawEvent> Message, StreamPosition Position)> _pending = [];

    private ulong? TrySkip(StreamedEvent<KurrentDBRawEvent> queueMessage, ulong? skippedTail)
    {
        // A skipped event still counts toward its transaction, so the rest of the transaction completes without it.
        if (_pending.Count > 0 && BelongsToPending(queueMessage))
        {
            _pendingSkipped++;
            return skippedTail;
        }
        return (ulong)queueMessage.Cursor.SequenceNumber;
    }

    private int _pendingSkipped;

    private bool BelongsToPending(StreamedEvent<KurrentDBRawEvent> queueMessage) =>
        _pending[0].Message.Metadata.TransactionId is { } transactionId && queueMessage.Metadata.TransactionId == transactionId;

    private bool IsTransactionComplete()
    {
        var size = _pending[0].Message.Metadata.TransactionSize;
        return _pending[0].Message.Metadata.TransactionId is null || size is null || _pending.Count + _pendingSkipped >= size.Value;
    }

    /// <summary>
    ///     Caches the pending transaction once under each stream its events belong to, holding that stream's events, and
    ///     once under each of its stream targets, holding every event. Every record is positioned at the last event, so
    ///     each stream sees the same sequence token for the transaction.
    /// </summary>
    private ulong? CacheTransaction(DateTime dequeueTimeUtc, List<CachedMessage> cachedMessages, List<StreamPosition> streamPositions, ulong? skippedTail)
    {
        var events = new StreamedEvent<KurrentDBRawEvent>[_pending.Count];
        for (var i = 0; i < _pending.Count; i++)
        {
            events[i] = _pending[i].Message;
        }
        var last = events[^1];
        var streams = new List<StreamId>();
        var eventsByStream = new Dictionary<StreamId, List<StreamedEvent<KurrentDBRawEvent>>>();
        foreach (var (message, position) in _pending)
        {
            if (!eventsByStream.TryGetValue(position.StreamId, out var streamEvents))
            {
                streamEvents = [];
                eventsByStream[position.StreamId] = streamEvents;
                streams.Add(position.StreamId);
            }
            streamEvents.Add(message);
        }
        var targets = GetTargetStreams(events, eventsByStream);
        _pending.Clear();
        _pendingSkipped = 0;
        var cachedAny = false;
        foreach (var streamId in streams)
        {
            if (eventsByStream.TryGetValue(streamId, out var streamEvents))
            {
                cachedAny |= TryCacheRecord(streamId, streamEvents, last, dequeueTimeUtc, cachedMessages, streamPositions);
            }
        }
        foreach (var streamId in targets)
        {
            cachedAny |= TryCacheRecord(streamId, events, last, dequeueTimeUtc, cachedMessages, streamPositions);
        }
        return cachedAny ? null : (ulong)last.Cursor.SequenceNumber;
    }

    private List<StreamId> GetTargetStreams(StreamedEvent<KurrentDBRawEvent>[] events, Dictionary<StreamId, List<StreamedEvent<KurrentDBRawEvent>>> eventsByStream)
    {
        var targets = new List<StreamId>();
        foreach (var message in events)
        {
            foreach (var target in message.Metadata.StreamTargets)
            {
                StreamId streamId;
                try
                {
                    streamId = _dataAdapter.GetTargetStreamId(target);
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidStreamNamespaceException)
                {
                    // A bad target must not cost the transaction its other streams.
                    _logger.InvalidStreamTarget(ex, message.EventId, message.StreamName.Value, message.Cursor.SequenceNumber, target);
                    continue;
                }
                // A target that is already a participant stream receives the whole transaction instead of its own part.
                eventsByStream.Remove(streamId);
                if (!targets.Contains(streamId))
                {
                    targets.Add(streamId);
                }
            }
        }
        return targets;
    }

    private bool TryCacheRecord(StreamId streamId, IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> events, StreamedEvent<KurrentDBRawEvent> last, DateTime dequeueTimeUtc,
        List<CachedMessage> cachedMessages, List<StreamPosition> streamPositions)
    {
        CachedMessage cachedMessage;
        try
        {
            cachedMessage = _dataAdapter.FromQueueMessages(streamId, events, last, dequeueTimeUtc, GetSegment);
        }
        catch (EventTooLargeForCacheException ex)
        {
            // Skipping would silently lose the transaction for every stream it belongs to, so the subscription halts
            // instead. Nothing of this batch has been cached, so no purge, and therefore no checkpoint, can pass it,
            // and the transaction is read again once the block size is raised and the silo restarted.
            _logger.TransactionTooLargeForCache(last.Metadata.TransactionId, events.Count, last.EventId, last.EventType, last.StreamName.Value, last.Cursor.SequenceNumber, streamId, ex.Size, ex.BlockSize);
            throw new KurrentDBTransactionTooLargeForCacheException(last.Metadata.TransactionId, (ulong)last.Cursor.SequenceNumber, ex.Size, ex.BlockSize, ex);
        }
        cachedMessages.Add(cachedMessage);
        streamPositions.Add(new StreamPosition(streamId, new EventSequenceToken(cachedMessage.SequenceNumber, 0)));
        return true;
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
                    // The event was just added, so this should not happen. Throwing would drop the batch's
                    // notifications even though its events are cached. The old cursor is left in place, and if it
                    // points into purged blocks its next read reports a cache miss to the pulling agent instead.
                    _logger.WakeCursorCacheMiss(cacheMiss.ToException(), streamId);
                    continue;
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
        // Sequence numbers are $all commit positions, which are byte offsets in the log rather than event counts, so the
        // item count decides whether the cache holds enough events to measure pressure.
        if (_cache.IsEmpty || !_cache.Newest.HasValue || !_cache.Oldest.HasValue || _cache.ItemCount < 10 * _defaultMaxAddCount)
        {
            // not enough items in cache.
            return false;
        }
        double cacheSize = _cache.Newest.Value.SequenceNumber - _cache.Oldest.Value.SequenceNumber;
        var distanceFromNewestMessage = _cache.Newest.Value.SequenceNumber - sequenceToken.SequenceNumber;
        // pressure is the ratio of the distance from the front of the cache to the size of the cache, both in bytes of log,
        // which approximates the share of cached events still ahead of the cursor
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
        (_evictionStrategy as KurrentDBCatchupEvictionStrategy)?.ReleaseBlocks();
        _buffer = null;
        (_bufferPool as KurrentDBQueueCacheBufferPool)?.UnregisterCache();
    }

    #endregion

}

internal static partial class KurrentDBCatchupQueueCacheLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "CachePeriod: EnqueueTimeUtc: {OldestEnqueueTimeUtc} to {NewestEnqueueTimeUtc}, DequeueTimeUtc: {OldestDequeueTimeUtc} to {NewestDequeueTimeUtc}")] public static partial void CachePeriod(this ILogger logger, string oldestEnqueueTimeUtc, string newestEnqueueTimeUtc, string oldestDequeueTimeUtc, string newestDequeueTimeUtc);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its cached size of {Size} bytes exceeds the cache block size of {BlockSize} bytes.")] public static partial void EventTooLargeForCache(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, int size, int blockSize);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its stream namespace '{StreamNamespace}' contains the reserved separator ':'.")] public static partial void InvalidStreamNamespace(this ILogger logger, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string streamNamespace);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipped event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition}: its metadata Orleans stream id '{OrleansStreamId}' is not a valid stream id.")] public static partial void InvalidOrleansStreamId(this ILogger logger, Exception exception, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, string orleansStreamId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Could not reposition a caught-up cursor on stream {StreamId} at the event just added to the cache.")] public static partial void WakeCursorCacheMiss(this ILogger logger, Exception exception, StreamId streamId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Ignored stream target '{StreamTarget}' of event {EventId} in stream {StreamName} at commit position {CommitPosition}: it does not map to a valid stream.")] public static partial void InvalidStreamTarget(this ILogger logger, Exception exception, Guid eventId, string streamName, System.Numerics.BigInteger commitPosition, string streamTarget);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Transaction {TransactionId} ending at position {Position} was only partially received ({Count} of {TransactionSize} events); delivering the events received.")] public static partial void PartialTransactionDelivered(this ILogger logger, Guid? transactionId, int count, int? transactionSize, ulong position);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Transaction {TransactionId} ending at position {Position} was only partially received ({Count} of {TransactionSize} events); delivering the events received. This is expected when the subscription filter excludes part of the transaction.")] public static partial void PartialTransactionDeliveredWithWarning(this ILogger logger, Guid? transactionId, int count, int? transactionSize, ulong position);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Halted: transaction {TransactionId} ending at position {Position} was only partially received ({Count} of {TransactionSize} events). Nothing past it has been delivered or checkpointed. Change the subscription filter, or set PartialTransactionHandling to Deliver or DeliverAndWarn, and restart.")] public static partial void PartialTransactionHalted(this ILogger logger, Guid? transactionId, int count, int? transactionSize, ulong position);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Halted: transaction {TransactionId} of {EventCount} events ending with event {EventId} of type {EventType} in stream {StreamName} at commit position {CommitPosition} needs {Size} bytes in the cache for stream {StreamId}, but the cache block size is {BlockSize} bytes. Raise the cache block size to at least {Size} bytes and restart; nothing past the transaction has been checkpointed.")] public static partial void TransactionTooLargeForCache(this ILogger logger, Guid? transactionId, int eventCount, Guid eventId, string eventType, string streamName, System.Numerics.BigInteger commitPosition, StreamId streamId, int size, int blockSize);
}
