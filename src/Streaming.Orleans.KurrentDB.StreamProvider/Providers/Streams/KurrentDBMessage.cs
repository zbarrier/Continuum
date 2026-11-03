using System.Buffers.Binary;

using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Orleans.KurrentDB;
using Continuum.Orleans.Streams;

using Orleans.Providers.Streams.Common;

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
    /// <param name="commitPosition"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="eventId"></param>
    /// <param name="eventType"></param>
    /// <param name="data"></param>
    /// <param name="streamName">The KurrentDB stream the event was written to.</param>
    /// <param name="streamVersion">The version of the event within its stream.</param>
    /// <param name="timestamp">The time the event was recorded.</param>
    /// <param name="metadata">The event metadata.</param>
    public KurrentDBMessage(string sourceConnectionName, StreamId streamId, ulong commitPosition, long sequenceNumber,
        Guid eventId, string eventType, ReadOnlyMemory<byte> data, StreamName streamName,
        ulong streamVersion = 0, DateTime timestamp = default, IEventMetadata? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceConnectionName);
        ArgumentNullException.ThrowIfNull(streamName);
        SourceConnectionName = sourceConnectionName;
        EventId = eventId;
        StreamId = streamId;
        StreamName = streamName;
        CommitPosition = commitPosition;
        StreamVersion = streamVersion;
        SequenceNumber = sequenceNumber;
        EventType = eventType;
        Data = data;
        Metadata = metadata ?? EventMetadata.Empty;
        Timestamp = timestamp;
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
        // Reads must follow the write order in KurrentDBQueueDataAdapter.EncodeMessageIntoSegment.
        var readOffset = 0;
        CommitPosition = ReadUInt64(cachedMessage.Segment, ref readOffset);
        EventId = new Guid(SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset).AsSpan());
        StreamId = cachedMessage.StreamId;
        StreamName = ReadStreamName(cachedMessage.Segment, ref readOffset);
        StreamVersion = ReadUInt64(cachedMessage.Segment, ref readOffset);
        SequenceNumber = cachedMessage.SequenceNumber;
        EventType = ReadRequiredString(cachedMessage.Segment, ref readOffset);
        Data = SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset);
        // Tracing and transaction values travel inside the metadata document rather than as their own fields.
        Metadata = KurrentDBEventMetadataCodec.Read(SegmentBuilder.ReadNextBytes(cachedMessage.Segment, ref readOffset).AsSpan());
        Timestamp = new DateTime(unchecked((long)ReadUInt64(cachedMessage.Segment, ref readOffset)), DateTimeKind.Utc);
    }

    internal static void WriteUInt64(ArraySegment<byte> segment, ref int writeOffset, ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(segment.AsSpan(writeOffset, sizeof(ulong)), value);
        writeOffset += sizeof(ulong);
    }

    internal static ulong ReadUInt64(ArraySegment<byte> segment, int offset) =>
        BinaryPrimitives.ReadUInt64LittleEndian(segment.AsSpan(offset, sizeof(ulong)));

    private static ulong ReadUInt64(ArraySegment<byte> segment, ref int readOffset)
    {
        var value = ReadUInt64(segment, readOffset);
        readOffset += sizeof(ulong);
        return value;
    }

    internal static string ReadRequiredString(ArraySegment<byte> segment, ref int readOffset) =>
        SegmentBuilder.ReadNextString(segment, ref readOffset)
        ?? throw new InvalidOperationException("Cached KurrentDB message segment contains an unexpected null string.");

    // The parsed parts are stored rather than re-derived on read, because recovering them means parsing the stream
    // name, and how a name splits is an application concern the cache has no parser for. A tenant id is never empty,
    // so an empty string stands for a name that is not tenant qualified.
    internal static int CalculateStreamNameSize(StreamName streamName) =>
        SegmentBuilder.CalculateAppendSize(streamName.Value) +
        SegmentBuilder.CalculateAppendSize(streamName.Category) +
        SegmentBuilder.CalculateAppendSize(streamName.TenantId ?? string.Empty) +
        SegmentBuilder.CalculateAppendSize(streamName.Key);

    internal static void WriteStreamName(ArraySegment<byte> segment, ref int writeOffset, StreamName streamName)
    {
        SegmentBuilder.Append(segment, ref writeOffset, streamName.Value);
        SegmentBuilder.Append(segment, ref writeOffset, streamName.Category);
        SegmentBuilder.Append(segment, ref writeOffset, streamName.TenantId ?? string.Empty);
        SegmentBuilder.Append(segment, ref writeOffset, streamName.Key);
    }

    internal static StreamName ReadStreamName(ArraySegment<byte> segment, ref int readOffset)
    {
        var value = ReadRequiredString(segment, ref readOffset);
        var category = ReadRequiredString(segment, ref readOffset);
        var tenantId = ReadRequiredString(segment, ref readOffset);
        var key = ReadRequiredString(segment, ref readOffset);
        return new StreamName(value, category, tenantId.Length == 0 ? null : tenantId, key);
    }

    /// <summary>
    ///     The logical connection the event was read from.
    /// </summary>
    [Id(0)]
    public string SourceConnectionName { get; set; } = string.Empty;

    /// <summary>
    ///     The id of the event in the stream.
    /// </summary>
    [Id(1)]
    public Guid EventId { get; set; }


    /// <summary>
    ///     The stream identifier.
    /// </summary>
    [Id(2)]
    public StreamId StreamId { get; set; }

    /// <summary>
    ///     The KurrentDB stream the event was read from, such as <c>snack-123</c>, with its parsed category, tenant and key.
    /// </summary>
    [Id(3)]
    public StreamName StreamName { get; set; }

    /// <summary>
    ///     The <c>$all</c> commit position the event was read at, used to write checkpoints after delivery.
    /// </summary>
    [Id(4)]
    public ulong CommitPosition { get; set; }

    /// <summary>
    ///     The event number of the event within its KurrentDB stream.
    /// </summary>
    [Id(5)]
    public ulong StreamVersion { get; set; }

    /// <summary>
    ///     The Orleans sequence number, which is <see cref="CommitPosition" /> narrowed to a signed 64-bit value.
    /// </summary>
    [Id(6)]
    public long SequenceNumber { get; set; }

    /// <summary>
    ///     The type name of the event.
    /// </summary>
    [Id(7)]
    public string EventType { get; set; }

    /// <summary>
    ///     The raw bytes representing the data of this event.
    /// </summary>
    /// <remarks>
    ///     Only the data adapter reads this, to materialize the event into the batch container's payload. It is
    ///     deliberately not serialized: once a batch container is copied or sent to a consumer this is empty, and the
    ///     materialized payload is the event's only representation, so the event is never carried twice.
    /// </remarks>
    [field: NonSerialized]
    public ReadOnlyMemory<byte> Data { get; set; }

    /// <summary>
    ///     The producer supplied metadata the event carried, with well known entries already lifted out.
    /// </summary>
    [Id(8)]
    public IEventMetadata Metadata { get; set; } = EventMetadata.Empty;

    /// <summary>
    ///     The time the event was created by its producer, in UTC.
    /// </summary>
    [Id(9)]
    public DateTime Timestamp { get; set; }
}
