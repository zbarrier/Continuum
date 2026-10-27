using System.Numerics;

namespace Continuum.Streaming;

/// <summary>
/// Where a streamed event was delivered from: the source connection, the topic and partition within it, and the event's
/// position within that partition.
/// </summary>
/// <remarks>
/// <para>
///     The cursor is provider neutral so that consumers can checkpoint and detect duplicates without knowing which
///     provider delivered the event. Each provider fills it from its own notion of position:
/// </para>
/// <list type="bullet">
///     <item>KurrentDB: the subscribed stream (for example <c>$all</c> or <c>$ce-order</c>), partition <c>0</c> and the commit position.</item>
///     <item>CosmosDB: the monitored container, the lease token and the logical sequence number (<c>_lsn</c>).</item>
///     <item>EventHub and Kafka: the topic, the partition id and the offset.</item>
///     <item>Kinesis: the stream, the shard id and the shard sequence number.</item>
/// </list>
/// <para>
///     <see cref="SequenceNumber"/> is a <see cref="BigInteger"/> because providers do not agree on a width; a Kinesis
///     sequence number does not fit in 64 bits. Positions are only comparable within the same source connection, topic
///     and partition.
/// </para>
/// <para>
///     <see cref="SourceConnectionName"/> names the database or broker the event was read from. Positions are local to
///     it: two databases can hand out the same position for unrelated events, so cursors from different sources have
///     no order relative to each other.
/// </para>
/// </remarks>
public interface IStreamedEventCursor
{
    /// <summary>
    /// The logical connection the event was read from. It is a permanent identifier: renaming it makes every position
    /// recorded under the old name unreachable.
    /// </summary>
    string SourceConnectionName { get; }

    /// <summary>
    /// The topic within <see cref="SourceConnectionName"/> the event was read from.
    /// </summary>
    string Topic { get; }

    /// <summary>
    /// The partition of <see cref="Topic"/> the event was read from.
    /// </summary>
    string Partition { get; }

    /// <summary>
    /// The position of the event within the partition.
    /// </summary>
    BigInteger SequenceNumber { get; }

    /// <summary>
    /// The order of the event among events sharing <see cref="SequenceNumber"/>.
    /// </summary>
    ulong SubSequenceNumber { get; }
}
