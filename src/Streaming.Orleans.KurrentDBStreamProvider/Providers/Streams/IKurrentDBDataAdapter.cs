using Continuum.Streaming.Orleans;

using KurrentDB.Client;

using Orleans.Providers.Streams.Common;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
/// </summary>
/// <remarks>
///     The queue message is an array because a batch is appended as one KurrentDB record per event, matching the
///     event sourcing storage. The records of a batch are written in a single atomic append and carry the transaction
///     metadata that identifies them as one group.
/// </remarks>
public interface IKurrentDBDataAdapter : IQueueDataAdapter<EventData[]>, ICacheDataAdapter
{
    /// <summary>
    /// </summary>
    /// <param name="position"></param>
    /// <param name="queueMessage"></param>
    /// <param name="dequeueTime"></param>
    /// <param name="getSegment"></param>
    /// <returns></returns>
    CachedMessage FromQueueMessage(StreamPosition position, StreamedEvent<KurrentDBRawEvent> queueMessage, DateTime dequeueTime, Func<int, ArraySegment<byte>> getSegment);

    /// <summary>
    ///     Get orleans stream position from the event message.
    /// </summary>
    /// <param name="queueMessage"></param>
    /// <returns></returns>
    StreamPosition GetStreamPosition(StreamedEvent<KurrentDBRawEvent> queueMessage);

    /// <summary>
    ///     Get KurrentDB event position from cached message.
    /// </summary>
    /// <param name="cachedMessage"></param>
    /// <returns></returns>
    string GetPosition(CachedMessage cachedMessage);

    /// <summary>
    ///     Get the log position a checkpoint should be written from, for a cached message.
    /// </summary>
    /// <param name="cachedMessage">The cached message.</param>
    /// <returns>
    ///     The <c>$all</c> commit position the event was read at. This is the value the checkpoint store holds, and it
    ///     is not recoverable from the event itself once link resolution is enabled.
    /// </returns>
    ulong GetCommitPosition(CachedMessage cachedMessage);

    /// <summary>
    ///     Get the <see cref="IStreamIdentity" /> for an event message.
    /// </summary>
    /// <param name="queueMessage">The event message.</param>
    /// <returns>The stream identity.</returns>
    StreamId GetStreamId(StreamedEvent<KurrentDBRawEvent> queueMessage);
}
