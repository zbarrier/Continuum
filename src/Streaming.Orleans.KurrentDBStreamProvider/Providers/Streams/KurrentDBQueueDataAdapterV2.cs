using System.Text;

using Continuum.Streaming;
using Continuum.Streaming.Orleans;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streams;

using MetadataCodec = global::Continuum.Streaming.Orleans.KurrentDB.KurrentDBEventMetadataCodec;
using StreamPosition = Orleans.Streams.StreamPosition;

using Continuum.Serialization.Orleans;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Converts event data to and from queue message。
///     Data adapter that uses types that support custom serializers (like json).
/// </summary>
public class KurrentDBQueueDataAdapterV2 : IKurrentDBDataAdapter
{
    private readonly Serializer _serializer;
    private readonly KurrentDBDataAdapterOptions _options;
    private readonly ILogger<KurrentDBQueueDataAdapterV2>? _logger;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _reportedHaltedEventIds = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueDataAdapterV2" /> class.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="options">The serialization boundary configuration for this stream provider.</param>
    /// <param name="logger">Optional logger used to report events that cannot be converted.</param>
    public KurrentDBQueueDataAdapterV2(Serializer serializer, KurrentDBDataAdapterOptions options, ILogger<KurrentDBQueueDataAdapterV2>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        _serializer = serializer;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    ///     Converts a cached message to a batch container for delivery
    /// </summary>
    /// <param name="cachedMessage"></param>
    /// <returns></returns>
    public virtual IBatchContainer GetBatchContainer(ref CachedMessage cachedMessage)
    {
        ArgumentNullException.ThrowIfNull(cachedMessage, nameof(cachedMessage));
        var evenStoreMessage = new KurrentDBMessage(cachedMessage);
        return GetBatchContainer(evenStoreMessage);
    }

    /// <summary>
    ///     Convert an KurrentDBMessage to a batch container
    /// </summary>
    /// <param name="kurrentDBMessage"></param>
    /// <returns></returns>
    protected virtual IBatchContainer GetBatchContainer(KurrentDBMessage kurrentDBMessage)
    {
        ArgumentNullException.ThrowIfNull(kurrentDBMessage, nameof(kurrentDBMessage));
        return new KurrentDBBatchContainerV2(kurrentDBMessage, DeserializeEvent(kurrentDBMessage));
    }

    /// <summary>
    ///     Materializes the domain event from the bytes the cache holds.
    /// </summary>
    /// <remarks>
    ///     The cache stores the event exactly as it was written, so the type is resolved here, at delivery, rather
    ///     than at ingest. An event whose type this silo does not know therefore fails only for its own stream instead
    ///     of stalling the queue, which matters because a silo reading <c>$all</c> sees every event on the log,
    ///     including those belonging to applications it knows nothing about.
    ///
    ///     A failure is returned rather than thrown. This runs inside the pooled cache while it advances a cursor, and
    ///     the cursor has already moved past this record; a throw here reaches the pulling agent as a cursor fault,
    ///     which it recovers from by rebuilding the cursor from the oldest cached event. Carrying the failure into the
    ///     batch container defers it to the delivery path, where Orleans retries and faults the subscription without
    ///     advancing progress.
    /// </remarks>
    protected KurrentDBEventMaterialization DeserializeEvent(KurrentDBMessage kurrentDBMessage)
    {
        if (kurrentDBMessage.Data.Length == 0)
        {
            _logger?.LogWarning("Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it has no payload.", kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
            return KurrentDBEventMaterialization.Suppressed();
        }
        if (!_options.TypeMapper.TryGetType(kurrentDBMessage.EventType, out var eventType))
        {
            var fault = $"The KurrentDB event of type \"{kurrentDBMessage.EventType}\" on stream \"{kurrentDBMessage.StreamId}\" could not be resolved. Register the event type with the type mapper for this stream provider.";
            if (_options.UnknownEventTypeBehavior == KurrentDBUnknownEventTypeBehavior.Skip)
            {
                _logger?.LogWarning("{Fault} The event was skipped because {Option} is {Behavior}.", fault, nameof(KurrentDBDataAdapterOptions.UnknownEventTypeBehavior), nameof(KurrentDBUnknownEventTypeBehavior.Skip));
                return KurrentDBEventMaterialization.Suppressed();
            }
            // Orleans retries a faulted delivery repeatedly and only logs its own warning, so the cause is reported
            // here as an error, once per event, to keep a halted subscription from looking merely slow.
            if (_reportedHaltedEventIds.TryAdd(kurrentDBMessage.EventId, 0))
            {
                _logger?.LogError("{Fault} Delivery is halted because {Option} is {Behavior}.", fault, nameof(KurrentDBDataAdapterOptions.UnknownEventTypeBehavior), nameof(KurrentDBUnknownEventTypeBehavior.Halt));
            }
            return KurrentDBEventMaterialization.Faulted(fault);
        }
        object? @event;
        try
        {
            @event = _options.GrainStorageSerializer.Deserialize<object>(new BinaryDataWithType(kurrentDBMessage.Data, eventType!));
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it could not be deserialized.", kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
            return KurrentDBEventMaterialization.Suppressed();
        }
        if (@event is null)
        {
            _logger?.LogWarning("Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it deserialized to null.", kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
            return KurrentDBEventMaterialization.Suppressed();
        }
        return KurrentDBEventMaterialization.Resolved(@event);
    }

    /// <summary>
    ///     Gets the stream sequence token from a cached message.
    /// </summary>
    /// <param name="cachedMessage"></param>
    /// <returns></returns>
    public virtual StreamSequenceToken GetSequenceToken(ref CachedMessage cachedMessage)
    {
        ArgumentNullException.ThrowIfNull(cachedMessage, nameof(cachedMessage));
        var readOffset = 0;
        var position = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        return new KurrentDBSequenceTokenV2(position, cachedMessage.SequenceNumber, cachedMessage.EventIndex);
    }

    /// <summary>
    ///     Creates the KurrentDB records for a batch of stream events.
    /// </summary>
    /// <typeparam name="T">The stream event type.</typeparam>
    /// <param name="streamId">The stream identifier.</param>
    /// <param name="events">The events.</param>
    /// <param name="sequenceToken">The sequence sequenceToken.</param>
    /// <param name="requestContext">The request context.</param>
    /// <returns>One record per event, appended together as a single transaction.</returns>
    public virtual EventData[] ToQueueMessage<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken sequenceToken, Dictionary<string, object> requestContext)
    {
        ArgumentNullException.ThrowIfNull(events, nameof(events));
        return KurrentDBEventDataFactory.ToEventData(streamId, events, _options);
    }

    /// <summary>
    ///     Creates a CachedMessage from a cloud queue message
    /// </summary>
    /// <returns>The message batch.</returns>
    public virtual CachedMessage FromQueueMessage(StreamPosition position, StreamedEvent<KurrentDBRawEvent> queueMessage, DateTime dequeueTime, Func<int, ArraySegment<byte>> getSegment)
    {
        ArgumentNullException.ThrowIfNull(position, nameof(position));
        ArgumentNullException.ThrowIfNull(queueMessage, nameof(queueMessage));
        ArgumentNullException.ThrowIfNull(getSegment, nameof(getSegment));
        return new CachedMessage
        {
            StreamId = position.StreamId,
            // Must agree with the token built in GetStreamPosition. The cache compares this against cursor tokens,
            // so a per-stream version here would collide across the many streams a $all receiver caches together.
            SequenceNumber = StreamSequenceNumber.ToOrleansSequenceToken(queueMessage.SequenceNumber, queueMessage.StreamName),
            EventIndex = position.SequenceToken.EventIndex,
            EnqueueTimeUtc = queueMessage.Timestamp,
            DequeueTimeUtc = dequeueTime,
            Segment = EncodeMessageIntoSegment(queueMessage, getSegment)
        };
    }

    /// <summary>
    ///     Get orleans stream position from the event message.
    /// </summary>
    /// <param name="queueMessage"></param>
    /// <returns></returns>
    /// <remarks>
    ///     The token is built from the <c>$all</c> commit position rather than the per-stream version. A receiver
    ///     reading <c>$all</c> caches events from many streams in one queue, and the cache orders and locates cached
    ///     messages by this sequence number; the stream version restarts at zero for every stream, so the first event
    ///     of each stream would share sequence number zero and the cache would treat them as the same position.
    /// </remarks>
    public virtual StreamPosition GetStreamPosition(StreamedEvent<KurrentDBRawEvent> queueMessage)
    {
        ArgumentNullException.ThrowIfNull(queueMessage, nameof(queueMessage));
        var streamId = GetStreamId(queueMessage);
        var sequenceToken = new KurrentDBSequenceTokenV2(queueMessage.SequenceNumber.ToString(), StreamSequenceNumber.ToOrleansSequenceToken(queueMessage.SequenceNumber, queueMessage.StreamName), 0);
        return new StreamPosition(streamId, sequenceToken);
    }

    /// <summary>
    ///     Get position from cached message.
    ///     Left to derived class, as only it knows how to get this from the cached message.
    /// </summary>
    public virtual string GetPosition(CachedMessage cachedMessage)
    {
        ArgumentNullException.ThrowIfNull(cachedMessage, nameof(cachedMessage));
        var readOffset = 0;
        return SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
    }

    /// <summary>
    ///     Get the <c>$all</c> commit position a checkpoint should be written from.
    /// </summary>
    public virtual ulong GetCommitPosition(CachedMessage cachedMessage)
    {
        ArgumentNullException.ThrowIfNull(cachedMessage, nameof(cachedMessage));
        var readOffset = 0;
        // Skip the stream position, the commit position is written straight after it.
        SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        return ulong.Parse(SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset));
    }

    /// <summary>
    ///     Get the <see cref="IStreamIdentity" /> for an event message.
    /// </summary>
    /// <param name="queueMessage">The event message.</param>
    /// <returns>The stream identity.</returns>
    public virtual StreamId GetStreamId(StreamedEvent<KurrentDBRawEvent> queueMessage)
    {
        ArgumentNullException.ThrowIfNull(queueMessage, nameof(queueMessage));
        // Events written by this provider record the Orleans stream id in their metadata, because every stream shares
        // one queue stream and the id cannot be recovered from the KurrentDB stream name. Events written by anyone
        // else have no such entry, so their identity comes from the stream name itself.
        var orleansStreamId = queueMessage.Metadata.GetValueOrDefault(MetadataCodec.OrleansStreamIdName);
        if (orleansStreamId is { Length: > 0 })
        {
            return StreamId.Parse(Encoding.UTF8.GetBytes(orleansStreamId));
        }
        return _options.StreamIdMapper(queueMessage.StreamName);
    }

    /// <summary>
    ///     Placed object message payload into a segment.
    /// </summary>
    /// <param name="queueMessage"></param>
    /// <param name="getSegment"></param>
    /// <returns></returns>
    protected virtual ArraySegment<byte> EncodeMessageIntoSegment(StreamedEvent<KurrentDBRawEvent> queueMessage, Func<int, ArraySegment<byte>> getSegment)
    {
        var position = queueMessage.StreamVersion.ToString();
        // The commit position is stored alongside the event so the checkpoint can be written from the cache, after
        // delivery. It cannot be recovered from the event itself once link resolution is enabled.
        var commitPosition = queueMessage.SequenceNumber.ToString();
        var eventId = queueMessage.EventId.ToString();
        var eventType = queueMessage.EventType;
        // The consumer is handed an IStreamedEvent rebuilt from the cache, so the fields it needs that are not
        // recoverable from CachedMessage travel with the event. PartitionId is deliberately absent: it is a fixed
        // constant for this provider and is supplied on read rather than stored once per cached event. Topic is not,
        // because it depends on the subscription strategy the receiver used, which the batch container cannot see.
        var streamName = queueMessage.StreamName;
        // The key is stored rather than re-derived on read, because recovering it means parsing the stream name,
        // and how a name splits is an application concern the batch container has no parser for.
        var streamKey = queueMessage.StreamKey;
        var topic = queueMessage.Topic;
        var streamVersion = queueMessage.StreamVersion.ToString();
        var timestamp = queueMessage.Timestamp.Ticks.ToString();
        // The metadata bag round trips as the same JSON document shape the codec reads from KurrentDB, including the
        // tracing and transaction values, so nothing the event carried is lost while it sits in the cache. The codec
        // returns null when there is nothing worth writing, which the segment represents as an empty string.
        var metadataBytes = MetadataCodec.WriteAll(queueMessage.Metadata);
        var metadata = metadataBytes is null ? string.Empty : Encoding.UTF8.GetString(metadataBytes);
        // The event is cached exactly as KurrentDB holds it. Both writers now produce the same record shape, a bare
        // serialized event under its mapped type name, so there is nothing to convert on the way in and an event whose
        // type this silo does not know can still be cached and simply fails for its own stream on read.
        ReadOnlySpan<byte> data = queueMessage.Event!.Data;

        // get total size.
        var size = SegmentBuilder.CalculateAppendSize(position) +
                   SegmentBuilder.CalculateAppendSize(commitPosition) +
                   SegmentBuilder.CalculateAppendSize(eventId) + 
                   SegmentBuilder.CalculateAppendSize(eventType) + 
                   SegmentBuilder.CalculateAppendSize(streamName) +
                   SegmentBuilder.CalculateAppendSize(streamKey) +
                   SegmentBuilder.CalculateAppendSize(topic) +
                   SegmentBuilder.CalculateAppendSize(streamVersion) +
                   SegmentBuilder.CalculateAppendSize(timestamp) +
                   SegmentBuilder.CalculateAppendSize(metadata) +
                   SegmentBuilder.CalculateAppendSize(data);

        // get segment
        var segment = getSegment(size);

        // encode
        var writeOffset = 0;
        SegmentBuilder.Append(segment, ref writeOffset, position);
        SegmentBuilder.Append(segment, ref writeOffset, commitPosition);
        SegmentBuilder.Append(segment, ref writeOffset, eventId);
        SegmentBuilder.Append(segment, ref writeOffset, eventType);
        SegmentBuilder.Append(segment, ref writeOffset, streamName);
        SegmentBuilder.Append(segment, ref writeOffset, streamKey);
        SegmentBuilder.Append(segment, ref writeOffset, topic);
        SegmentBuilder.Append(segment, ref writeOffset, streamVersion);
        SegmentBuilder.Append(segment, ref writeOffset, timestamp);
        SegmentBuilder.Append(segment, ref writeOffset, metadata);
        SegmentBuilder.Append(segment, ref writeOffset, data);

        return segment;
    }
}
