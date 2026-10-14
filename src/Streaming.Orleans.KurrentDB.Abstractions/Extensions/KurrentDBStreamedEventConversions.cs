using Continuum.Orleans.KurrentDB;
using KurrentDB.Client;
using Continuum.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.Extensions;

/// <summary>
///     Converts KurrentDB events and positions to <see cref="StreamedEvent{T}" /> instances.
/// </summary>
public static class KurrentDBStreamedEventConversions
{
    /// <summary>
    ///     The topic of checkpoint events read from the <c>$all</c> stream.
    /// </summary>
    public const string AllStreamCheckpointTopic = KurrentDBStreams.All;

    /// <summary>
    ///     The topic of checkpoint events read from a single stream.
    /// </summary>
    public const string StreamCheckpointTopic = "DefaultTopic";

    /// <summary>
    ///     The partition identifier assigned to KurrentDB events, which are not partitioned.
    /// </summary>
    public const string DefaultPartitionId = "0";

    /// <summary>
    ///     The payload carried by checkpoint events, which mark progress through a stream but hold no event of their own.
    /// </summary>
    public static readonly object CheckpointMarker = new();

    /// <summary>
    ///     The stream name carried by checkpoint events, which mark progress through a subscription but belong to no stream.
    /// </summary>
    public static readonly Continuum.Orleans.Streams.StreamName CheckpointStreamName = new("$checkpoint", string.Empty, null, "$checkpoint");

    private static readonly IStreamNameParser DefaultNameParser = new DefaultKurrentDBStreamNameParser();

    /// <summary>
    ///     Creates the cursor for an event read from KurrentDB.
    /// </summary>
    /// <param name="streamName">The stream the event was written to.</param>
    /// <param name="sequenceNumber">The position the event was read at.</param>
    /// <returns>A cursor whose topic is the stream category and whose partition is the individual stream.</returns>
    /// <remarks>
    ///     KurrentDB orders every event in one log, but positions only reach a consumer in order within a single
    ///     stream: Orleans delivers each stream on its own and does not order one stream against another, even when
    ///     they share a queue. Scoping the partition to the stream keeps a consumer that deduplicates by partition
    ///     from discarding an event of one stream because a later event of another stream arrived first. The tenant
    ///     is part of the partition so that the same key under two tenants is never treated as one stream.
    /// </remarks>
    public static StreamedEventCursor ToCursor(IStreamName streamName, System.Numerics.BigInteger sequenceNumber)
    {
        ArgumentNullException.ThrowIfNull(streamName);
        var partition = streamName.TenantId is null ? streamName.Key : $"{streamName.TenantId}|{streamName.Key}";
        return new StreamedEventCursor(streamName.Category, partition, sequenceNumber, 0);
    }

    /// <summary>
    ///     Creates a checkpoint event for a position in the <c>$all</c> stream.
    /// </summary>
    /// <param name="position">The <c>$all</c> position.</param>
    /// <returns>An event carrying <see cref="CheckpointMarker" /> as its payload.</returns>
    public static StreamedEvent<object> ToAllStreamCheckpointEvent(this Position position)
    {
        var streamEvent = new StreamedEvent<object>(
            Guid.CreateVersion7(),
            "AllStreamCheckpointEventType",
            CheckpointStreamName,
            0,
            new StreamedEventCursor(AllStreamCheckpointTopic, DefaultPartitionId, position.CommitPosition, 0),
            DateTime.UtcNow,
            CheckpointMarker);
        return streamEvent;
    }

    /// <summary>
    ///     Creates a streamed event from an event read from the <c>$all</c> stream.
    /// </summary>
    /// <param name="resolvedEvent">The KurrentDB event.</param>
    /// <param name="deserializedEvent">The deserialized payload.</param>
    /// <param name="nameParser">The parser for the event's stream name, or <see langword="null" /> for <see cref="DefaultKurrentDBStreamNameParser" />.</param>
    /// <returns>The streamed event.</returns>
    public static StreamedEvent<object> ToAllStreamEvent(this ResolvedEvent resolvedEvent, object deserializedEvent, IStreamNameParser? nameParser = null)
    {
        var streamName = (nameParser ?? DefaultNameParser).Parse(resolvedEvent.Event.EventStreamId);
        var metadata = KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span);
        var streamEvent = new StreamedEvent<object>(
            resolvedEvent.Event.EventId.ToGuid(),
            resolvedEvent.Event.EventType,
            streamName,
            resolvedEvent.Event.EventNumber,
            // When link resolution is enabled the $all position of the link is carried by OriginalPosition;
            // Event.Position is the position of the linked-to event, which lives elsewhere in the log and
            // would resume the subscription from the wrong place.
            ToCursor(streamName, (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition),
            resolvedEvent.Event.Created,
            deserializedEvent,
            metadata);
        return streamEvent;
    }

    /// <summary>
    ///     Creates a checkpoint event for a position in a single stream.
    /// </summary>
    /// <param name="position">The stream position.</param>
    /// <returns>An event carrying <see cref="CheckpointMarker" /> as its payload.</returns>
    public static StreamedEvent<object> ToStreamCheckpointEvent(this StreamPosition position)
    {
        var streamEvent = new StreamedEvent<object>(
            Guid.CreateVersion7(),
            "StreamCheckpointEventType",
            CheckpointStreamName,
            0,
            new StreamedEventCursor(StreamCheckpointTopic, DefaultPartitionId, position.ToUInt64(), 0),
            DateTime.UtcNow,
            CheckpointMarker);
        return streamEvent;
    }

    /// <summary>
    ///     Creates a streamed event from an event read from a single stream.
    /// </summary>
    /// <param name="resolvedEvent">The KurrentDB event.</param>
    /// <param name="deserializedEvent">The deserialized payload.</param>
    /// <param name="nameParser">The parser for the event's stream name, or <see langword="null" /> for <see cref="DefaultKurrentDBStreamNameParser" />.</param>
    /// <returns>The streamed event.</returns>
    public static StreamedEvent<object> ToStreamEvent(this ResolvedEvent resolvedEvent, object deserializedEvent, IStreamNameParser? nameParser = null)
    {
        var streamName = (nameParser ?? DefaultNameParser).Parse(resolvedEvent.Event.EventStreamId);
        var metadata = KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span);
        var streamEvent = new StreamedEvent<object>(
            resolvedEvent.Event.EventId.ToGuid(),
            resolvedEvent.Event.EventType,
            streamName,
            resolvedEvent.Event.EventNumber,
            // A single-stream subscription is positioned by the event number within the subscribed stream, which
            // differs from the linked-to event's own version when link resolution is enabled.
            ToCursor(streamName, resolvedEvent.OriginalEventNumber.ToUInt64()),
            resolvedEvent.Event.Created,
            deserializedEvent,
            metadata);
        return streamEvent;
    }
}
