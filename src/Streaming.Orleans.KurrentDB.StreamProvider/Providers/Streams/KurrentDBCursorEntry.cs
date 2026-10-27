using Orleans.Runtime;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     A stable handle for a cache cursor. The underlying Orleans cache cursor can be replaced when a caught-up cursor
///     is woken by new events, so the handle given to consumers never changes.
/// </summary>
public sealed class KurrentDBCursorEntry
{
    internal KurrentDBCursorEntry(StreamId streamId, object cacheCursor)
    {
        StreamId = streamId;
        CacheCursor = cacheCursor;
    }

    /// <summary>
    ///     The stream this cursor reads.
    /// </summary>
    public StreamId StreamId { get; }

    /// <summary>
    ///     The Orleans cache cursor currently backing this entry.
    /// </summary>
    internal object CacheCursor { get; set; }

    /// <summary>
    ///     The commit position of the last event this cursor has been handed.
    /// </summary>
    public ulong CommitPosition { get; internal set; }

    /// <summary>
    ///     Whether this cursor has read every cached event for its stream and so does not hold back purging.
    /// </summary>
    public bool IsCaughtUp { get; internal set; }

    /// <summary>
    ///     Whether this cursor has been released.
    /// </summary>
    public bool IsReleased { get; internal set; }
}
