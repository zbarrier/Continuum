using Continuum.Events;
using Continuum.Streams;

namespace Continuum.Streaming;

/// <summary>
/// An event delivered from a stream, together with its position and metadata.
/// </summary>
/// <typeparam name="T">The type of the event payload.</typeparam>
public interface IStreamedEvent<out T> where T : class
{
    /// <summary>
    /// Unique sequential identifier for the event.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The type of the event data.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// The stream the event belongs to, together with the category, tenant and key parsed out of it.
    /// <para>For KurrentDB, this is the original stream id, for example <c>order-123</c>.</para>
    /// <para>For CosmosDB, this is the stream name used as the container's partition key.</para>
    /// <para>For EventHub, Kafka and Kinesis, this is the partition key.</para>
    /// </summary>
    IStreamName StreamName { get; }

    /// <summary>
    /// The event number or version of the event within its <see cref="StreamName"/>.
    /// </summary>
    ulong StreamVersion { get; }

    /// <summary>
    /// Where the event was delivered from: the topic and partition of the source and the event's position within it.
    /// Consumers use this to checkpoint and to detect events they have already processed.
    /// </summary>
    IStreamedEventCursor Cursor { get; }

    /// <summary>
    /// The time of the event being enqueued/produced in UTC.
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Producer supplied metadata describing the event, including its tracing context and the transaction it was
    /// appended in.
    /// <para>
    ///     Values are strings because metadata exists to be read without deserializing the event; structured data
    ///     belongs in <see cref="Event"/>. Well known entries such as tracing and transaction data are lifted onto
    ///     typed properties of <see cref="IEventMetadata"/> rather than left as raw entries.
    /// </para>
    /// <para>Empty rather than <see langword="null"/> when the event carried no metadata.</para>
    /// </summary>
    IEventMetadata Metadata { get; }

    /// <summary>
    /// Deserialized event.
    /// </summary>
    T Event { get; }
}