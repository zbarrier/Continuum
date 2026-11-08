using Continuum.Streaming.Orleans;

using KurrentDB.Client;

using Orleans.Providers.Streams.Common;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

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
    ///     Creates one cached record holding every event of a transaction, positioned at the commit position of its
    ///     last event.
    /// </summary>
    /// <param name="streamId">The stream the record is cached under.</param>
    /// <param name="queueMessages">The events of the transaction, in log order.</param>
    /// <param name="positionedAt">The last event of the transaction, whose commit position the record is positioned at.</param>
    /// <param name="dequeueTime">When the events were dequeued.</param>
    /// <param name="getSegment">Allocates cache memory.</param>
    CachedMessage FromQueueMessages(StreamId streamId, IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> queueMessages, StreamedEvent<KurrentDBRawEvent> positionedAt, DateTime dequeueTime, Func<int, ArraySegment<byte>> getSegment);

    /// <summary>
    ///     Gets the stream a transaction is fanned out to for a stream target recorded in event metadata.
    /// </summary>
    /// <param name="streamTarget">The target, the text form of a grain id.</param>
    /// <returns>The stream identity, with its namespace qualified by the provider name.</returns>
    StreamId GetTargetStreamId(string streamTarget);

    /// <summary>
    ///     Get orleans stream position from the event message.
    /// </summary>
    /// <param name="queueMessage"></param>
    /// <returns></returns>
    StreamPosition GetStreamPosition(StreamedEvent<KurrentDBRawEvent> queueMessage);

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
