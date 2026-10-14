using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Streams;
using Continuum.Orleans.Streams;

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
    /// <param name="streamName">The stream the event belongs to.</param>
    /// <param name="streamVersion">The version of the event within its stream.</param>
    /// <param name="cursor">Where the event was delivered from.</param>
    /// <param name="timestamp">The time the event was recorded.</param>
    /// <param name="event">The event payload.</param>
    /// <param name="metadata">The event metadata, or <see langword="null"/> for none.</param>
    /// <exception cref="ArgumentNullException"><paramref name="event"/> is <see langword="null"/>.</exception>
    public StreamedEvent(Guid eventId, string eventType, IStreamName streamName, ulong streamVersion, IStreamedEventCursor cursor,
        DateTime timestamp, T @event, IEventMetadata? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(@event);

        EventId = eventId;
        EventType = eventType;
        StreamName = StreamName.From(streamName);
        StreamVersion = streamVersion;
        Cursor = StreamedEventCursor.From(cursor);
        Timestamp = timestamp;
        Event = @event;
        Metadata = metadata ?? EventMetadata.Empty;
    }

    /// <inheritdoc />
    [Id(0)]
    public Guid EventId { get; init; }

    /// <inheritdoc />
    [Id(1)]
    public string EventType { get; init; }

    /// <inheritdoc />
    [Id(2)]
    public StreamName StreamName { get; init; }

    IStreamName IStreamedEvent<T>.StreamName => StreamName;

    /// <inheritdoc />
    [Id(3)]
    public ulong StreamVersion { get; init; }

    /// <inheritdoc />
    [Id(4)]
    public StreamedEventCursor Cursor { get; init; }

    IStreamedEventCursor IStreamedEvent<T>.Cursor => Cursor;

    /// <inheritdoc />
    [Id(5)]
    public DateTime Timestamp { get; init; }

    /// <inheritdoc />
    [Id(6)]
    public T Event { get; init; }

    /// <inheritdoc />
    [Id(7)]
    public IEventMetadata Metadata { get; init; } = EventMetadata.Empty;
}
