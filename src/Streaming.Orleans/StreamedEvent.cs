using System.Numerics;

namespace Continuum.Streaming.Orleans;

[Alias("Continuum.Streaming.StreamedEvent.V1`1"), GenerateSerializer, Immutable]
public sealed class StreamedEvent<T> : IStreamedEvent<T> where T : class
{
    public StreamedEvent(NewId eventId, string eventType, string streamName, string streamKey, string topic, string partitionId,
        ulong streamVersion, ulong streamPosition, BigInteger sequenceNumber, ulong subSequenceNumber, DateTime timestamp, 
        T? evt, IStreamedEventMetadata? metadata = null)
    {
        EventId = eventId;
        EventType = eventType;
        StreamName = streamName;
        StreamKey = streamKey;
        Topic = topic;
        PartitionId = partitionId;
        StreamVersion = streamVersion;
        StreamPosition = streamPosition;
        SequenceNumber = sequenceNumber;
        SubSequenceNumber = subSequenceNumber;
        Timestamp = timestamp;
        Event = evt;
        Metadata = metadata ?? StreamedEventMetadata.Empty;
    }

    /// <inheritdoc />
    [Id(0)]
    public NewId EventId { get; init; }

    /// <inheritdoc />
    [Id(1)]
    public string EventType { get; init; }

    /// <inheritdoc />
    [Id(2)]
    public string StreamName { get; init; }

    /// <inheritdoc />
    [Id(3)]
    public string StreamKey { get; }

    /// <inheritdoc />
    [Id(4)]
    public string Topic { get; init; }

    /// <inheritdoc />
    [Id(5)]
    public string PartitionId { get; }

    /// <inheritdoc />
    [Id(6)]
    public ulong StreamVersion { get; init; }

    /// <inheritdoc />
    [Id(7)]
    public ulong StreamPosition { get; init; }

    /// <inheritdoc />
    [Id(8)]
    public BigInteger SequenceNumber { get; init; }

    /// <inheritdoc />
    [Id(9)]
    public ulong SubSequenceNumber { get; init; }

    /// <inheritdoc />
    [Id(10)]
    public DateTime Timestamp { get; init; }

    /// <inheritdoc />
    [Id(11)]
    public T? Event { get; init; }

    /// <inheritdoc />
    [Id(12)]
    public IStreamedEventMetadata Metadata { get; init; } = StreamedEventMetadata.Empty;
}
