using KurrentDB.Client;

namespace Continuum.Streaming.Orleans.KurrentDB.Extensions;

public static class StreamEventExtensions
{
    public const string AllStreamTopic = "$all";
    public const string DefaultTopic = "DefaultTopic";
    public const string DefaultPartitionId = "0";

    // Checkpoint events carry no stream of their own, so they keep a fixed topic. Real events are read through a
    // parser instead, because the topic they report has to be the category the store groups them under.
    private static readonly IStreamedNameParser NameParser = new DefaultKurrentDBStreamNameParser();

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
            null);
        return streamEvent;
    }

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
            null);
        return streamEvent;
    }

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
