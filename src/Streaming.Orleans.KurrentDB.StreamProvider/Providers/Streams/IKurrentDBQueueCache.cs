using System.Diagnostics.CodeAnalysis;
using Continuum.Streaming.Orleans;

using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

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
    /// <remarks>
    ///     A null token starts at the newest event. A token older than the oldest cached event starts at the oldest
    ///     cached event only when nothing for the stream was purged after that token; otherwise events would be skipped,
    ///     so a <see cref="QueueCacheMissException" /> is thrown and the consumer must catch up from KurrentDB itself.
    /// </remarks>
    object GetCursor(StreamId streamId, StreamSequenceToken? sequenceToken);

    /// <summary>
    ///     Get a cursor into the cache at a subscription start position.
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="startPosition"></param>
    /// <returns></returns>
    /// <remarks>
    ///     <see cref="StreamSubscriptionStartPosition.EarliestAvailable" /> starts at the oldest cached event for the
    ///     stream and never reports a cache miss, so it covers the gap around the time of subscribing without requiring
    ///     the consumer to know a position.
    /// </remarks>
    object GetCursorAtPosition(StreamId streamId, StreamSubscriptionStartPosition startPosition);

    /// <summary>
    ///     Try to get the next queue messages in the cache for the provided cursor.
    /// </summary>
    /// <param name="cursorObj"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    bool TryGetNextMessage(object cursorObj, [MaybeNullWhen(false)] out IBatchContainer container);

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
