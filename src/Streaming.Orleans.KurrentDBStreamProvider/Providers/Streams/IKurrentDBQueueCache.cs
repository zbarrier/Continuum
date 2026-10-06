using Continuum.Streaming.Orleans;

using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Interface for a stream queue messages cache that stores KurrentDB events
/// </summary>
public interface IKurrentDBQueueCache : IQueueFlowController, IDisposable
{
    /// <summary>
    ///     Add a list of KurrentDB events to the cache.
    /// </summary>
    /// <param name="queueMessages"></param>
    /// <param name="dequeueTimeUtc"></param>
    /// <returns></returns>
    List<StreamPosition> Add(List<StreamedEvent<KurrentDBRawEvent>> queueMessages, DateTime dequeueTimeUtc);

    /// <summary>
    ///     Get a cursor into the cache to read events from a stream.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="sequenceToken"></param>
    /// <returns></returns>
    object GetCursor(StreamId streamId, StreamSequenceToken sequenceToken);

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
    ///     Raised with the <c>$all</c> commit position of the newest event that has been purged, and is therefore
    ///     behind every active cursor.
    /// </summary>
    Action<ulong>? OnCheckpointablePosition { get; set; }

    /// <summary>
    ///     Try to get the next queue messages in the cache for the provided cursor.
    /// </summary>
    /// <param name="cursorObj"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    bool TryGetNextMessage(object cursorObj, out IBatchContainer container);

    /// <summary>
    ///     Add cache pressure monitor to the cache's back pressure algorithm
    /// </summary>
    /// <param name="monitor"></param>
    void AddCachePressureMonitor(ICachePressureMonitor monitor);

    /// <summary>
    ///     Send purge signal to the cache, the cache will perform a time based purge on its cached messages
    /// </summary>
    void SignalPurge();
}
