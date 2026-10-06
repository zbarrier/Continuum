using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
///     Converts an <see cref="IStreamedEvent{T}.SequenceNumber" /> into the fixed-width integers that some transports
///     and checkpoint stores require.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="IStreamedEvent{T}.SequenceNumber" /> is a <see cref="BigInteger" /> because the providers this
///         abstraction targets do not agree on a width. A KurrentDB commit position, a CosmosDB <c>_lsn</c> and an
///         EventHub or Kafka offset all fit in 64 bits, but an AWS Kinesis sequence number does not: it is a decimal
///         string far wider than <see cref="ulong" />. Narrowing the abstraction to a fixed-width integer would make
///         Kinesis unrepresentable, so the width is kept open here and constrained only where a specific transport
///         demands it.
///     </para>
///     <para>
///         The conversions below are those constraint points. They exist so that a value which cannot be represented
///         fails with a message naming the sequence number and the limit it exceeded, rather than as a bare
///         <see cref="OverflowException" /> raised from an unattributed cast deep in a delivery path.
///     </para>
/// </remarks>
public static class StreamSequenceNumber
{
    /// <summary>
    ///     Converts a sequence number to the <see cref="long" /> an Orleans stream sequence token orders by.
    /// </summary>
    /// <param name="sequenceNumber">The provider supplied sequence number.</param>
    /// <param name="streamName">The stream the event belongs to, used only to describe a failure.</param>
    /// <returns>The sequence number as a <see cref="long" />.</returns>
    /// <exception cref="NotSupportedException">
    ///     The sequence number does not fit in a <see cref="long" />, so it cannot be carried by an Orleans stream
    ///     sequence token.
    /// </exception>
    /// <remarks>
    ///     Orleans' <c>EventSequenceToken</c> stores a <see cref="long" /> and compares cursors with it, so an Orleans
    ///     stream provider can only order events whose sequence numbers fall in that range. This is the documented
    ///     limit of the Orleans streaming path: a provider whose positions exceed <see cref="long.MaxValue" />, such as
    ///     Kinesis, has to be consumed through the catch-up subscription path instead, which compares sequence numbers
    ///     as <see cref="BigInteger" /> values and never narrows them.
    /// </remarks>
    public static long ToOrleansSequenceToken(BigInteger sequenceNumber, string streamName)
    {
        if (sequenceNumber < long.MinValue || sequenceNumber > long.MaxValue)
        {
            throw new NotSupportedException(
                $"The sequence number {sequenceNumber} on stream \"{streamName}\" cannot be represented in an Orleans " +
                $"stream sequence token, which orders events by a signed 64-bit integer with a maximum of {long.MaxValue}. " +
                "A provider whose positions exceed this range must be consumed through a catch-up subscription, which " +
                "compares sequence numbers without narrowing them.");
        }

        return (long)sequenceNumber;
    }

    /// <summary>
    ///     Converts a sequence number to the <see cref="ulong" /> a KurrentDB commit position is expressed in.
    /// </summary>
    /// <param name="sequenceNumber">The provider supplied sequence number.</param>
    /// <param name="streamName">The stream the event belongs to, used only to describe a failure.</param>
    /// <returns>The sequence number as a <see cref="ulong" />.</returns>
    /// <exception cref="NotSupportedException">
    ///     The sequence number does not fit in a <see cref="ulong" />, so it is not a KurrentDB commit position.
    /// </exception>
    /// <remarks>
    ///     A KurrentDB commit position is a byte offset into the transaction log and is always a <see cref="ulong" />.
    ///     Reaching this limit is not expected; the check is here so that a sequence number originating from a
    ///     different provider is reported as the mismatch it is instead of silently wrapping into a position that
    ///     would move a checkpoint backwards.
    /// </remarks>
    public static ulong ToCommitPosition(BigInteger sequenceNumber, string streamName)
    {
        if (sequenceNumber < ulong.MinValue || sequenceNumber > ulong.MaxValue)
        {
            throw new NotSupportedException(
                $"The sequence number {sequenceNumber} on stream \"{streamName}\" cannot be represented as a KurrentDB " +
                $"commit position, which is an unsigned 64-bit integer with a maximum of {ulong.MaxValue}.");
        }

        return (ulong)sequenceNumber;
    }
}
