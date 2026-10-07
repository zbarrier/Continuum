using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.Extensions;

/// <summary>
///     Converts KurrentDB events and positions to <see cref="StreamedEvent{T}" /> instances.
/// </summary>
public static class StreamEventExtensions
{
    /// <summary>
    ///     The topic of checkpoint events read from the <c>$all</c> stream.
    /// </summary>
    public const string AllStreamTopic = "$all";

    /// <summary>
    ///     The topic of checkpoint events read from a single stream.
    /// </summary>
    public const string DefaultTopic = "DefaultTopic";

    /// <summary>
    ///     The partition identifier assigned to KurrentDB events, which are not partitioned.
    /// </summary>
    public const string DefaultPartitionId = "0";

    /// <summary>
    ///     The payload carried by checkpoint events, which mark progress through a stream but hold no event of their own.
    /// </summary>
    public static readonly object CheckpointMarker = new();

    // Checkpoint events carry no stream of their own, so they keep a fixed topic. Real events are read through a
    // parser instead, because the topic they report has to be the category the store groups them under.
    private static readonly IStreamedNameParser NameParser = new DefaultKurrentDBStreamNameParser();

    /// <summary>
    ///     Creates a checkpoint event for a position in the <c>$all</c> stream.
    /// </summary>
    /// <param name="position">The <c>$all</c> position.</param>
    /// <returns>An event carrying <see cref="CheckpointMarker" /> as its payload.</returns>
    public static StreamedEvent<object> ToAllStreamCheckpointEvent(this Position position)
    {
        var streamEvent = new StreamedEvent<object>(
            NewId.Next(),
            "AllStreamCheckpointEventType",
            string.Empty,
            "",
            AllStreamTopic,
            DefaultPartitionId,
            0,
            0,
            position.CommitPosition,
            0,
            DateTime.UtcNow,
            CheckpointMarker);
        return streamEvent;
    }

    /// <summary>
    ///     Creates a streamed event from an event read from the <c>$all</c> stream.
    /// </summary>
    /// <param name="resolvedEvent">The KurrentDB event.</param>
    /// <param name="deserializedEvent">The deserialized payload.</param>
    /// <returns>The streamed event.</returns>
    public static StreamedEvent<object> ToAllStreamEvent(this ResolvedEvent resolvedEvent, object deserializedEvent)
    {
        // The topic is the event's own category, not the $all stream it happened to be read from. A subscription
        // to $all is a fan-out over every category, so reporting $all here would give two unrelated streams the
        // same topic and leave a consumer keyed by topic unable to tell them apart.
        var parsed = NameParser.Parse(resolvedEvent.Event.EventStreamId);
        var metadata = KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span);
        var streamEvent = new StreamedEvent<object>(
            NewId.FromSequentialGuid(resolvedEvent.Event.EventId.ToGuid()),
            resolvedEvent.Event.EventType,
            resolvedEvent.Event.EventStreamId,
            parsed.StreamKey,
            parsed.Topic,
            DefaultPartitionId,
            resolvedEvent.Event.EventNumber,
            resolvedEvent.OriginalEventNumber.ToUInt64(),
            // When link resolution is enabled the $all position of the link is carried by OriginalPosition;
            // Event.Position is the position of the linked-to event, which lives elsewhere in the log and
            // would resume the subscription from the wrong place.
            (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition,
            0,
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
            NewId.Next(),
            "StreamCheckpointEventType",
            string.Empty,
            string.Empty,
            DefaultTopic,
            DefaultPartitionId,
            0,
            position.ToUInt64(),
            0,
            0,
            DateTime.UtcNow,
            CheckpointMarker);
        return streamEvent;
    }

    /// <summary>
    ///     Creates a streamed event from an event read from a single stream.
    /// </summary>
    /// <param name="resolvedEvent">The KurrentDB event.</param>
    /// <param name="deserializedEvent">The deserialized payload.</param>
    /// <returns>The streamed event.</returns>
    public static StreamedEvent<object> ToStreamEvent(this ResolvedEvent resolvedEvent, object deserializedEvent)
    {
        var parsed = NameParser.Parse(resolvedEvent.Event.EventStreamId);
        var metadata = KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span);
        var streamEvent = new StreamedEvent<object>(
            NewId.FromSequentialGuid(resolvedEvent.Event.EventId.ToGuid()),
            resolvedEvent.Event.EventType,
            resolvedEvent.Event.EventStreamId,
            parsed.StreamKey,
            parsed.Topic,
            DefaultPartitionId,
            resolvedEvent.Event.EventNumber,
            resolvedEvent.OriginalEventNumber.ToUInt64(),
            resolvedEvent.Event.Position.CommitPosition,
            0,
            resolvedEvent.Event.Created,
            deserializedEvent,
            metadata);
        return streamEvent;
    }
}
