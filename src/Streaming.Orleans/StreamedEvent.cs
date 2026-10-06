using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// The Orleans-serializable <see cref="IStreamedEvent{T}"/> delivered to stream subscribers.
/// </summary>
/// <typeparam name="T">The type of the event payload.</typeparam>
[Alias("Continuum.Streaming.StreamedEvent.V1`1"), GenerateSerializer, Immutable]
public sealed class StreamedEvent<T> : IStreamedEvent<T> where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamedEvent{T}"/> class.
    /// </summary>
    /// <param name="eventId">The unique event identifier.</param>
    /// <param name="eventType">The mapped event type name.</param>
    /// <param name="streamName">The stream the event was read from.</param>
    /// <param name="streamKey">The stream key.</param>
    /// <param name="topic">The topic or container of the subscription.</param>
    /// <param name="partitionId">The topic partition identifier.</param>
    /// <param name="streamVersion">The version of the event within its stream.</param>
    /// <param name="streamPosition">The position of the event within the subscribed stream.</param>
    /// <param name="sequenceNumber">The sequence number of the event within the topic partition.</param>
    /// <param name="subSequenceNumber">The order of the event among events sharing <paramref name="sequenceNumber"/>.</param>
    /// <param name="timestamp">The time the event was recorded.</param>
    /// <param name="evt">The event payload.</param>
    /// <param name="metadata">The event metadata, or <see langword="null"/> for none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="evt"/> is <see langword="null"/>.</exception>
    public StreamedEvent(NewId eventId, string eventType, string streamName, string streamKey, string topic, string partitionId,
        ulong streamVersion, ulong streamPosition, BigInteger sequenceNumber, ulong subSequenceNumber, DateTime timestamp,
        T evt, IStreamedEventMetadata? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(evt);

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
    public string StreamKey { get; init; }

    /// <inheritdoc />
    [Id(4)]
    public string Topic { get; init; }

    /// <inheritdoc />
    [Id(5)]
    public string PartitionId { get; init; }

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
    public T Event { get; init; }

    /// <inheritdoc />
    [Id(12)]
    public IStreamedEventMetadata Metadata { get; init; } = StreamedEventMetadata.Empty;
}
