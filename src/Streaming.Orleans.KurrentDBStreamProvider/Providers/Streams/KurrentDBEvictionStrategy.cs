using Microsoft.Extensions.Logging;

using Orleans.Providers.Streams.Common;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     A <see cref="ChronologicalEvictionStrategy" /> that additionally refuses to purge events which have not been
///     read by every active cursor.
/// </summary>
/// <remarks>
///     The checkpoint for the <c>$all</c> strategy is written from the purge callback, so purging is the point that
///     declares an event durably handled. The base strategy purges on age alone, which would let a slow consumer's
///     unread events be checkpointed away and lost on restart. Adding the cursor watermark makes purge, and therefore
///     the checkpoint, delivery safe while leaving the age based limits intact.
/// </remarks>
internal sealed class KurrentDBEvictionStrategy : ChronologicalEvictionStrategy
{
    private readonly IKurrentDBDataAdapter _dataAdapter;
    private readonly KurrentDBCursorTracker _cursorTracker;

    public KurrentDBEvictionStrategy(ILogger logger, TimePurgePredicate timePurge, ICacheMonitor? cacheMonitor, TimeSpan? monitorWriteInterval,
        IKurrentDBDataAdapter dataAdapter, KurrentDBCursorTracker cursorTracker)
        : base(logger, timePurge, cacheMonitor, monitorWriteInterval)
    {
        ArgumentNullException.ThrowIfNull(dataAdapter, nameof(dataAdapter));
        ArgumentNullException.ThrowIfNull(cursorTracker, nameof(cursorTracker));
        _dataAdapter = dataAdapter;
        _cursorTracker = cursorTracker;
    }

    /// <inheritdoc />
    protected override bool ShouldPurge(ref CachedMessage cachedMessage, ref CachedMessage newestCachedMessage, DateTime nowUtc)
    {
        if (!base.ShouldPurge(ref cachedMessage, ref newestCachedMessage, nowUtc))
        {
            return false;
        }
        var slowestCommitPosition = _cursorTracker.GetSlowestCommitPosition();
        if (slowestCommitPosition is null)
        {
            // No active cursors, so nothing can still be waiting on this event.
            return true;
        }
        // Only purge events the slowest cursor has already been handed.
        return _dataAdapter.GetCommitPosition(cachedMessage) <= slowestCommitPosition.Value;
    }
}
