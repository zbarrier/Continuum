using System.Numerics;

namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
///     Converts a provider-neutral <see cref="IStreamedEventCursor"/> back into a KurrentDB position.
/// </summary>
public static class StreamedEventCursorExtensions
{
    /// <summary>
    ///     Returns the cursor's <see cref="IStreamedEventCursor.SequenceNumber"/> as a KurrentDB commit position or
    ///     stream revision, both of which are unsigned 64-bit integers.
    /// </summary>
    /// <param name="cursor">The cursor of an event delivered from KurrentDB.</param>
    /// <param name="streamName">The stream the event belongs to, used to give an out-of-range error context.</param>
    /// <returns>The KurrentDB position.</returns>
    /// <exception cref="NotSupportedException">The sequence number is negative or wider than 64 bits.</exception>
    public static ulong ToKurrentDBPosition(this IStreamedEventCursor cursor, string streamName)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        return ToKurrentDBPosition(cursor.SequenceNumber, streamName);
    }

    /// <summary>
    ///     Returns <paramref name="sequenceNumber"/> as a KurrentDB commit position or stream revision.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="sequenceNumber"/> is negative or wider than 64 bits.</exception>
    public static ulong ToKurrentDBPosition(BigInteger sequenceNumber, string streamName)
    {
        if (sequenceNumber < ulong.MinValue || sequenceNumber > ulong.MaxValue)
        {
            throw new NotSupportedException(
                $"The sequence number {sequenceNumber} on stream \"{streamName}\" cannot be represented as a KurrentDB " +
                $"position, which is an unsigned 64-bit integer with a maximum of {ulong.MaxValue}.");
        }

        return (ulong)sequenceNumber;
    }
}
