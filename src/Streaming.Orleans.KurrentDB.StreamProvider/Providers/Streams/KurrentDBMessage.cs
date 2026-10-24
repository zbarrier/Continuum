using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.KurrentDB;
using System.Text;

using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;

using Orleans.Providers.Streams.Common;

using MetadataCodec = Continuum.Orleans.KurrentDB.KurrentDBEventMetadataCodec;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

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
    /// <param name="sourceConnectionName">The logical connection the event was read from.</param>
    /// <param name="streamId"></param>
    /// <param name="position"></param>
    /// <param name="commitPosition"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="eventId"></param>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    /// <param name="streamName">The KurrentDB stream the event was written to.</param>
    /// <param name="streamVersion">The version of the event within its stream.</param>
    /// <param name="timestamp">The time the event was recorded.</param>
    /// <param name="metadata">The event metadata.</param>
    /// <param name="streamKey">The stream key parsed from the stream name.</param>
    public KurrentDBMessage(string sourceConnectionName, StreamId streamId, string position, ulong commitPosition, long sequenceNumber,
        Guid eventId, string eventType, ReadOnlyMemory<byte> data,
        string? streamName = null, ulong streamVersion = 0, DateTime timestamp = default,
        IEventMetadata? metadata = null, string? streamKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceConnectionName);
        SourceConnectionName = sourceConnectionName;
        StreamId = streamId;
        Position = position;
        CommitPosition = commitPosition;
        SequenceNumber = sequenceNumber;
        EventId = eventId;
        EventType = eventType;
        Data = data;
        StreamName = streamName ?? string.Empty;
        StreamKey = streamKey ?? string.Empty;
        StreamVersion = streamVersion;
        Timestamp = timestamp;
        Metadata = metadata ?? EventMetadata.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBMessage" /> class.
    /// </summary>
    /// <param name="cachedMessage"></param>
    /// <param name="sourceConnectionName">The logical connection the cached event was read from.</param>
    public KurrentDBMessage(CachedMessage cachedMessage, string sourceConnectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceConnectionName);
        SourceConnectionName = sourceConnectionName;
        var readOffset = 0;
        StreamId = cachedMessage.StreamId;
        Position = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        CommitPosition = ulong.Parse(ReadRequiredString(cachedMessage.Segment, ref readOffset));
        SequenceNumber = cachedMessage.SequenceNumber;
        EventId = new Guid(SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset).AsSpan());
        EventType = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        StreamName = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        StreamCategory = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        var tenantId = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        TenantId = tenantId.Length == 0 ? null : tenantId;
        StreamKey = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        StreamVersion = ulong.Parse(ReadRequiredString(cachedMessage.Segment, ref readOffset));
        Timestamp = new DateTime(long.Parse(ReadRequiredString(cachedMessage.Segment, ref readOffset)), DateTimeKind.Utc);
        // Tracing and transaction values travel inside the metadata document rather than as their own fields.
        Metadata = MetadataCodec.Read(Encoding.UTF8.GetBytes(ReadRequiredString(cachedMessage.Segment, ref readOffset)));
        Data = SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset);
    }

    internal static string ReadRequiredString(ArraySegment<byte> segment, ref int readOffset) =>
        SegmentBuilder.ReadNextString(segment, ref readOffset)
        ?? throw new InvalidOperationException("Cached KurrentDB message segment contains an unexpected null string.");

    /// <summary>
    ///     The logical connection the event was read from.
    /// </summary>
    [Id(20)]
    public string SourceConnectionName { get; set; } = string.Empty;

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
    ///     The id of the event in the stream.
    /// </summary>
    [Id(5)]
    public Guid EventId { get; set; }

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
    ///     The category parsed from the stream name. For <c>snack-123</c> this is <c>snack</c>.
    /// </summary>
    [Id(18)]
    public string StreamCategory { get; set; } = string.Empty;

    /// <summary>
    ///     The tenant parsed from the stream name, or <see langword="null" /> when it is not tenant qualified.
    /// </summary>
    [Id(19)]
    public string? TenantId { get; set; }

    /// <summary>
    ///     The parsed stream name, or <see langword="null" /> when the message carries no stream name.
    /// </summary>
    public Continuum.Orleans.Streams.StreamName ToStreamName()
        => new(StreamName, StreamCategory, TenantId, StreamKey.Length == 0 ? StreamName : StreamKey);

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
    public IEventMetadata Metadata { get; set; } = EventMetadata.Empty;
}
