using System.Numerics;

namespace Continuum.Streaming;
public interface IStreamedEvent<out T> where T : class
{
    /// <summary>
    /// Unique sequential identifier for the event.
    /// </summary>
    NewId EventId { get; }

    /// <summary>
    /// The type of the event data.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// The stream name from where the message was read. Typically, comes in a form similar to {StreamCategory}/{StreamKey}.
    /// <para>For EventStoreDB, this will be the OriginalStreamId.</para>
    /// <para>For CosmosDB, this will be the StreamName which is the PartitionKey for the container.</para>
    /// <para>For EventHub and Kafka, this will be $"{Topic}/{PartitionKey}".</para>
    /// <para>For Kinesis, this will be the $"{Stream}/{PartitionKey}".</para>
    /// </summary>
    string StreamName { get; }

    /// <summary>
    /// The stream key or partition key. If the stream name is Order-123, the key will be 123.
    /// </summary>
    string StreamKey { get; }

    /// <summary>
    /// The topic or container for the subscription.
    /// <para>For EventStoreDB, this will be the category or '$all' for the $all stream.</para>
    /// <para>For CosmosDB, this will be the name of the monitored container.</para>
    /// <para>For EventHub and Kafka, this will be the Topic.</para>
    /// <para>For Kinesis, this will be the Stream.</para>
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// The identifier of the topic's partition.
    /// <para>For EventStoreDB, this will be '0' catch-up subscriptions.</para>
    /// <para>For CosmosDB, this will be the leaseToken (i.e. '0').</para>
    /// <para>For EventHub and Kafka, this will be the PartitionId.</para>
    /// <para>For Kinesis, this will be the ShardId.</para>
    /// </summary>
    string PartitionId { get; }

    /// <summary>
    /// The event number or version of an event sourced aggregate/stream.
    /// </summary>
    ulong StreamVersion { get; }

    /// <summary>
    /// The position within the stream.
    /// <para>
    ///     For EventStoreDB, it is the position of the event within the subscription. 
    ///     It can differ from <see cref="StreamVersion"/> when reading from a category or the $all stream.
    /// </para>
    /// <para>
    ///     For CosmosDB, it will be the same as <see cref="StreamVersion"/>.
    /// </para>
    /// <para>
    ///     For EventHub, Kafka and Kinesis it will be the same as <see cref="StreamVersion"/>.
    /// </para>
    /// </summary>
    ulong StreamPosition { get; }

    /// <summary>
    /// The commit position within an event store or the offset within a topic partition.
    /// <para>For EventStoreDB, it will be the global commit position.</para>
    /// <para>For CosmosDB, it will be the LogicalSequenceNumber (_lsn), which is the transaction sequence number per lease token.</para>
    /// <para>For EventHub and Kafka, it will be the Offset.</para>
    /// <para>For Kinesis, it will be the shard sequence number, which is wider than 64 bits.</para>
    /// <para>
    ///     This is a <see cref="BigInteger"/> because the supported providers do not agree on a width. Most fit in 64
    ///     bits, but a Kinesis sequence number does not, so narrowing this would make Kinesis unrepresentable.
    /// </para>
    /// <para>
    ///     Consumers reached through an Orleans stream provider are the exception: Orleans orders stream cursors by the
    ///     <see cref="long"/> in its <c>EventSequenceToken</c>, so only sequence numbers within <see cref="long"/> range
    ///     can be delivered that way. Providers exceeding it, Kinesis among them, must be consumed through a catch-up
    ///     subscription, which compares sequence numbers as <see cref="BigInteger"/> values and never narrows them.
    ///     See <see cref="StreamSequenceNumber"/>, where that conversion is guarded.
    /// </para>
    /// </summary>
    BigInteger SequenceNumber { get; }

    /// <summary>
    /// Used when multiple events are packed together with the same <see cref="SequenceNumber"/>.
    /// </summary>
    ulong SubSequenceNumber { get; }

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
    ///     typed properties of <see cref="IStreamedEventMetadata"/> rather than left as raw entries.
    /// </para>
    /// <para>Empty rather than <see langword="null"/> when the event carried no metadata.</para>
    /// </summary>
    IStreamedEventMetadata Metadata { get; }

    /// <summary>
    /// Deserialized event.
    /// </summary>
    T Event { get; }
}