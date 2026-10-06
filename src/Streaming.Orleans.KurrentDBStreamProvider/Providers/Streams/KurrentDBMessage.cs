using System.Text;

using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;

using Orleans.Providers.Streams.Common;

using MetadataCodec = Continuum.Streaming.Orleans.KurrentDB.KurrentDBEventMetadataCodec;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Replication of KurrentDB EventData And EventRecord class,
///     reconstructed from cached data CachedKurrentDBMessage
/// </summary>
[Serializable]
[GenerateSerializer]
public class KurrentDBMessage
{
    /// <summary>
    /// </summary>
    /// <param name="streamId"></param>
    /// <param name="position"></param>
    /// <param name="commitPosition"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="enqueueTimeUtc"></param>
    /// <param name="dequeueTimeUtc"></param>
    /// <param name="eventId"></param>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    public KurrentDBMessage(StreamId streamId, string position, ulong commitPosition, long sequenceNumber, DateTime enqueueTimeUtc, DateTime dequeueTimeUtc, 
        string eventId, string eventType, ReadOnlyMemory<byte> data,
        string? streamName = null, ulong streamVersion = 0, DateTime timestamp = default,
        IStreamedEventMetadata? metadata = null, string? topic = null, string? streamKey = null)
    {
        StreamId = streamId;
        Position = position;
        CommitPosition = commitPosition;
        SequenceNumber = sequenceNumber;
        EnqueueTimeUtc = enqueueTimeUtc;
        DequeueTimeUtc = dequeueTimeUtc;
        EventId = eventId;
        EventType = eventType;
        Data = data;
        StreamName = streamName ?? string.Empty;
        StreamKey = streamKey ?? string.Empty;
        Topic = topic ?? string.Empty;
        StreamVersion = streamVersion;
        Timestamp = timestamp;
        Metadata = metadata ?? StreamedEventMetadata.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBMessage" /> class.
    /// </summary>
    /// <param name="cachedMessage"></param>
    public KurrentDBMessage(CachedMessage cachedMessage)
    {
        var readOffset = 0;
        StreamId = cachedMessage.StreamId;
        Position = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        CommitPosition = ulong.Parse(SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset));
        SequenceNumber = cachedMessage.SequenceNumber;
        EnqueueTimeUtc = cachedMessage.EnqueueTimeUtc;
        DequeueTimeUtc = cachedMessage.DequeueTimeUtc;
        EventId = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        EventType = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        StreamName = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        StreamKey = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        Topic = SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset);
        StreamVersion = ulong.Parse(SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset));
        Timestamp = new DateTime(long.Parse(SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset)), DateTimeKind.Utc);
        // Tracing and transaction values travel inside the metadata document rather than as their own fields.
        Metadata = MetadataCodec.Read(Encoding.UTF8.GetBytes(SegmentBuilder.ReadNextString(cachedMessage.Segment, ref readOffset)));
        Data = SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset);
    }

    /// <summary>
    ///     The stream identifier.
    /// </summary>
    [Id(0)]
    public StreamId StreamId { get; set; }

    /// <summary>
    ///     Referring to a potential logical record position in the KurrentDB transaction file.
    /// </summary>
    [Id(1)]
    public string Position { get; set; }

    /// <summary>
    ///     The <c>$all</c> commit position the event was read at, used to write checkpoints after delivery.
    /// </summary>
    [Id(8)]
    public ulong CommitPosition { get; set; }

    /// <summary>
    ///     The position of the event in the stream.
    /// </summary>
    [Id(2)]
    public long SequenceNumber { get; set; }
    /// <summary>
    ///     The time message was written to the message queue.
    /// </summary>

    [Id(3)]
    public DateTime EnqueueTimeUtc { get; set; }

    /// <summary>
    ///     The time this message was read from the message queue.
    /// </summary>
    [Id(4)]
    public DateTime DequeueTimeUtc { get; set; }

    /// <summary>
    ///     The id of the event in the stream.
    /// </summary>
    [Id(5)]
    public string EventId { get; set; }

    /// <summary>
    ///     The type name of the event.
    /// </summary>
    [Id(6)]
    public string EventType { get; set; }

    /// <summary>
    ///     The raw bytes representing the data of this event.
    /// </summary>
    [Id(7)]
    public ReadOnlyMemory<byte> Data { get; set; }

    /// <summary>
    ///     The KurrentDB stream the event was read from, such as <c>snack-123</c>.
    /// </summary>
    [Id(12)]
    public string StreamName { get; set; } = string.Empty;

    /// <summary>
    ///     The key identifying the individual stream within its category. For <c>snack-123</c> this is <c>123</c>.
    /// </summary>
    [Id(17)]
    public string StreamKey { get; set; } = string.Empty;

    /// <summary>
    ///     The category the stream belongs to. For <c>snack-123</c> this is <c>snack</c>.
    /// </summary>
    [Id(16)]
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    ///     The event number of the event within its KurrentDB stream.
    /// </summary>
    [Id(13)]
    public ulong StreamVersion { get; set; }

    /// <summary>
    ///     The time the event was created by its producer, in UTC.
    /// </summary>
    [Id(14)]
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     The producer supplied metadata the event carried, with well known entries already lifted out.
    /// </summary>
    [Id(15)]
    public IStreamedEventMetadata Metadata { get; set; } = StreamedEventMetadata.Empty;
}
