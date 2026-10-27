using System.Buffers;
using System.Text;

using Continuum.Orleans.KurrentDB;
using Continuum.Orleans.Streams;
using Continuum.Serialization.Orleans;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Converts event data to and from queue message。
///     Data adapter that uses types that support custom serializers (like json).
/// </summary>
public class KurrentDBQueueDataAdapter : IKurrentDBDataAdapter, IDisposable
{
    private readonly Serializer _serializer;
    private readonly KurrentDBDataAdapterOptions _options;
    private readonly ILogger<KurrentDBQueueDataAdapter>? _logger;
    private readonly HaltedEventReporter _haltedEventReporter = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBQueueDataAdapter" /> class.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="options">The serialization boundary configuration for this stream provider.</param>
    /// <param name="logger">Optional logger used to report events that cannot be converted.</param>
    public KurrentDBQueueDataAdapter(Serializer serializer, KurrentDBDataAdapterOptions options, ILogger<KurrentDBQueueDataAdapter>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        _serializer = serializer;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the resources used by the adapter.
    /// </summary>
    /// <param name="disposing"><see langword="true" /> when called from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _haltedEventReporter.Dispose();
        }
    }

    /// <summary>
    ///     Converts a cached message to a batch container for delivery
    /// </summary>
    /// <param name="cachedMessage"></param>
    /// <returns></returns>
    public virtual IBatchContainer GetBatchContainer(ref CachedMessage cachedMessage)
    {
        var kurrentDBMessage = new KurrentDBMessage(cachedMessage, _options.ConnectionName);
        return GetBatchContainer(kurrentDBMessage);
    }

    /// <summary>
    ///     Convert an KurrentDBMessage to a batch container
    /// </summary>
    /// <param name="kurrentDBMessage"></param>
    /// <returns></returns>
    protected virtual IBatchContainer GetBatchContainer(KurrentDBMessage kurrentDBMessage)
    {
        ArgumentNullException.ThrowIfNull(kurrentDBMessage, nameof(kurrentDBMessage));
        return new KurrentDBBatchContainer(kurrentDBMessage, DeserializeEvent(kurrentDBMessage));
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
            _logger?.EventHasNoPayload(kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
            return KurrentDBEventMaterialization.Suppressed();
        }
        if (!_options.TypeMapper.TryGetType(kurrentDBMessage.EventType, out var eventType))
        {
            var fault = $"The KurrentDB event of type \"{kurrentDBMessage.EventType}\" on stream \"{kurrentDBMessage.StreamId}\" could not be resolved. Register the event type with the type mapper for this stream provider.";
            if (_options.UnknownEventTypeBehavior == KurrentDBUnknownEventTypeBehavior.Skip)
            {
                _logger?.UnknownEventTypeSkipped(fault, nameof(KurrentDBDataAdapterOptions.UnknownEventTypeBehavior), nameof(KurrentDBUnknownEventTypeBehavior.Skip));
                return KurrentDBEventMaterialization.Suppressed();
            }
            // Orleans retries a faulted delivery repeatedly and only logs its own warning, so the cause is reported
            // here as an error, once per event, to keep a halted subscription from looking merely slow.
            if (_haltedEventReporter.ShouldReport(kurrentDBMessage.EventId))
            {
                _logger?.UnknownEventTypeHalted(fault, nameof(KurrentDBDataAdapterOptions.UnknownEventTypeBehavior), nameof(KurrentDBUnknownEventTypeBehavior.Halt));
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
            _logger?.EventDeserializationFailed(ex, kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
            return KurrentDBEventMaterialization.Suppressed();
        }
        if (@event is null)
        {
            _logger?.EventDeserializedToNull(kurrentDBMessage.EventType, kurrentDBMessage.StreamId);
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
        return new EventSequenceToken(cachedMessage.SequenceNumber, cachedMessage.EventIndex);
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
    public virtual EventData[] ToQueueMessage<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken? sequenceToken, Dictionary<string, object>? requestContext)
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
            SequenceNumber = ToOrleansSequenceToken(queueMessage.Cursor.SequenceNumber, queueMessage.StreamName.Value),
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
        var sequenceToken = new EventSequenceToken(ToOrleansSequenceToken(queueMessage.Cursor.SequenceNumber, queueMessage.StreamName.Value), 0);
        return new StreamPosition(streamId, sequenceToken);
    }

    /// <summary>
    ///     Get the <c>$all</c> commit position a checkpoint should be written from.
    /// </summary>
    public virtual ulong GetCommitPosition(CachedMessage cachedMessage)
    {
        // The commit position is written first because eviction reads it for every cached message.
        return KurrentDBMessage.ReadUInt64(cachedMessage.Segment, 0);
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
        var orleansStreamId = queueMessage.Metadata.GetValueOrDefault(KurrentDBEventMetadataCodec.OrleansStreamIdName);
        if (orleansStreamId is { Length: > 0 })
        {
            return ParseStreamId(orleansStreamId);
        }
        return _options.StreamIdMapper(queueMessage.StreamName.Value);
    }

    private const int MaxStackallocStreamIdBytes = 256;

    private static StreamId ParseStreamId(string orleansStreamId)
    {
        // Encodes into a stack or pooled buffer so each message does not allocate a throwaway byte array.
        var byteCount = Encoding.UTF8.GetByteCount(orleansStreamId);
        if (byteCount <= MaxStackallocStreamIdBytes)
        {
            Span<byte> buffer = stackalloc byte[MaxStackallocStreamIdBytes];
            var written = Encoding.UTF8.GetBytes(orleansStreamId, buffer);
            return StreamId.Parse(buffer[..written]);
        }

        var rented = ArrayPool<byte>.Shared.Rent(byteCount);
        try
        {
            var written = Encoding.UTF8.GetBytes(orleansStreamId, rented);
            return StreamId.Parse(rented.AsSpan(0, written));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    ///     Placed object message payload into a segment.
    /// </summary>
    /// <param name="queueMessage"></param>
    /// <param name="getSegment"></param>
    /// <returns></returns>
    protected virtual ArraySegment<byte> EncodeMessageIntoSegment(StreamedEvent<KurrentDBRawEvent> queueMessage, Func<int, ArraySegment<byte>> getSegment)
    {
        var eventId = queueMessage.EventId.ToByteArray();
        // The consumer is handed an IStreamedEvent rebuilt from the cache, so the fields it needs that are not
        // recoverable from CachedMessage travel with the event. The cursor's topic and partition are absent: they are
        // the stream category and stream, so they are rebuilt on read from the stream name stored below.
        var streamName = StreamName.From(queueMessage.StreamName);
        // The commit position is stored alongside the event so the checkpoint can be written from the cache, after
        // delivery. It cannot be recovered from the event itself once link resolution is enabled.
        var commitPosition = (ulong)queueMessage.Cursor.SequenceNumber;
        var streamVersion = queueMessage.StreamVersion;
        var eventType = queueMessage.EventType;
        // The event is cached exactly as KurrentDB holds it. Both writers now produce the same record shape, a bare
        // serialized event under its mapped type name, so there is nothing to convert on the way in and an event whose
        // type this silo does not know can still be cached and simply fails for its own stream on read.
        ReadOnlySpan<byte> data = queueMessage.Event!.Data;
        // The metadata bag round trips as the same JSON document shape the codec reads from KurrentDB, including the
        // tracing and transaction values, so nothing the event carried is lost while it sits in the cache. The codec
        // returns null when there is nothing worth writing, which the segment represents as no bytes.
        ReadOnlySpan<byte> metadata = KurrentDBEventMetadataCodec.WriteAll(queueMessage.Metadata);
        var timestamp = queueMessage.Timestamp.Ticks;

        // get total size.
        var size = sizeof(ulong) +
                   SegmentBuilder.CalculateAppendSize(eventId) +
                   KurrentDBMessage.CalculateStreamNameSize(streamName) +
                   sizeof(ulong) +
                   SegmentBuilder.CalculateAppendSize(eventType) +
                   SegmentBuilder.CalculateAppendSize(data) +
                   SegmentBuilder.CalculateAppendSize(metadata) +
                   sizeof(long);

        // get segment
        var segment = getSegment(size);

        // encode. The order must match the reads in the KurrentDBMessage cached message constructor. The commit
        // position comes first, at a fixed width, so GetCommitPosition can read it without walking the segment.
        // Numbers are written as fixed-width binary so reading them back does not parse text.
        var writeOffset = 0;
        KurrentDBMessage.WriteUInt64(segment, ref writeOffset, commitPosition);
        SegmentBuilder.Append(segment, ref writeOffset, eventId);
        KurrentDBMessage.WriteStreamName(segment, ref writeOffset, streamName);
        KurrentDBMessage.WriteUInt64(segment, ref writeOffset, streamVersion);
        SegmentBuilder.Append(segment, ref writeOffset, eventType);
        SegmentBuilder.Append(segment, ref writeOffset, data);
        SegmentBuilder.Append(segment, ref writeOffset, metadata);
        KurrentDBMessage.WriteUInt64(segment, ref writeOffset, unchecked((ulong)timestamp));

        return segment;
    }

    /// <summary>
    ///     Narrows a KurrentDB position to the signed 64-bit sequence number the Orleans queue cache orders by.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="sequenceNumber"/> does not fit in a <see cref="long"/>.</exception>
    internal static long ToOrleansSequenceToken(System.Numerics.BigInteger sequenceNumber, string streamName)
    {
        if (sequenceNumber < long.MinValue || sequenceNumber > long.MaxValue)
        {
            throw new NotSupportedException(
                $"The sequence number {sequenceNumber} on stream \"{streamName}\" cannot be represented in an Orleans " +
                $"stream sequence token, which orders events by a signed 64-bit integer with a maximum of {long.MaxValue}.");
        }

        return (long)sequenceNumber;
    }
}

internal static partial class KurrentDBQueueDataAdapterLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it has no payload.")] public static partial void EventHasNoPayload(this ILogger logger, string eventType, StreamId streamId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Fault} The event was skipped because {Option} is {Behavior}.")] public static partial void UnknownEventTypeSkipped(this ILogger logger, string fault, string option, string behavior);

    [LoggerMessage(Level = LogLevel.Error, Message = "{Fault} Delivery is halted because {Option} is {Behavior}.")] public static partial void UnknownEventTypeHalted(this ILogger logger, string fault, string option, string behavior);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it could not be deserialized.")] public static partial void EventDeserializationFailed(this ILogger logger, Exception exception, string eventType, StreamId streamId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipping KurrentDB event of type \"{EventType}\" on stream \"{StreamId}\" because it deserialized to null.")] public static partial void EventDeserializedToNull(this ILogger logger, string eventType, StreamId streamId);
}
