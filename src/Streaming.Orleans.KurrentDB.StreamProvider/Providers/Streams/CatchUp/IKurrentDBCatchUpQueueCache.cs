namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

/// <summary>
///     A queue cache for catch-up subscriptions, which tracks how far each cursor has read so purging, and therefore
///     the client-side <c>$all</c> checkpoint, never advances past an event a consumer has not received.
/// </summary>
public interface IKurrentDBCatchUpQueueCache : IKurrentDBQueueCache
{
    /// <summary>
    ///     Stop tracking a cursor that is no longer reading, so it no longer holds back purging.
    /// </summary>
    /// <param name="cursorObj"></param>
    void ReleaseCursor(object cursorObj);

    /// <summary>
    ///     Records that a cursor has finished with the event at <paramref name="commitPosition" />, so the cache may
    ///     purge, and therefore checkpoint, past it.
    /// </summary>
    /// <remarks>
    ///     Called once the event has been delivered rather than when it was handed out, so an event that failed to
    ///     deliver keeps holding the watermark down and is still available after a restart.
    /// </remarks>
    /// <param name="cursorObj"></param>
    /// <param name="commitPosition"></param>
    void RecordDelivered(object cursorObj, ulong commitPosition);

    /// <summary>
    ///     Records that a cursor has no pending events for its stream, so it has read everything currently cached that it
    ///     could ever need and no longer holds back purging of other streams.
    /// </summary>
    /// <param name="cursorObj"></param>
    void RecordCaughtUp(object cursorObj);

    /// <summary>
    ///     Raised with the <c>$all</c> commit position of the newest event that has been purged, and is therefore
    ///     behind every active cursor.
    /// </summary>
    Action<ulong>? OnCheckpointablePosition { get; set; }
}
