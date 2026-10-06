using System.Collections.Concurrent;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Tracks how far each active cache cursor has read so the cache never purges, and therefore never checkpoints,
///     past an event that some consumer has not seen yet.
/// </summary>
/// <remarks>
///     The Orleans pooled cache purges purely on age, and the checkpoint is written from the purge callback. Without
///     this watermark a slow consumer would have its unread events aged out and checkpointed away, so a silo restart
///     would skip them. Progress is tracked by <c>$all</c> commit position because that is the only value that orders
///     events across every stream in the cache.
/// </remarks>
public sealed class KurrentDBCursorTracker
{
    private readonly ConcurrentDictionary<object, ulong> _cursors = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    ///     Registers a newly created cursor. Until it reads anything it holds the watermark at zero, which blocks all
    ///     purging; that is the safe direction, since a cursor that has read nothing may still need the oldest event.
    /// </summary>
    public void Register(object cursor)
    {
        _cursors[cursor] = 0;
    }

    /// <summary>
    ///     Records that a cursor has been handed the event at <paramref name="commitPosition" />.
    /// </summary>
    public void RecordProgress(object cursor, ulong commitPosition)
    {
        _cursors[cursor] = commitPosition;
    }

    /// <summary>
    ///     Removes a disposed cursor so it stops holding the watermark down.
    /// </summary>
    public void Unregister(object cursor)
    {
        _cursors.TryRemove(cursor, out _);
    }

    /// <summary>
    ///     The commit position every active cursor has read up to, or <see langword="null" /> when there are no
    ///     cursors and purging is unrestricted.
    /// </summary>
    public ulong? GetSlowestCommitPosition()
    {
        var slowest = default(ulong?);
        foreach (var position in _cursors.Values)
        {
            if (slowest is null || position < slowest)
            {
                slowest = position;
            }
        }
        return slowest;
    }
}
