using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// The position of an event within a topic partition, ordered by <see cref="SequenceNumber"/> and then by
/// <see cref="SubSequenceNumber"/>.
/// </summary>
/// <param name="SequenceNumber">The sequence number of the event.</param>
/// <param name="SubSequenceNumber">The order of the event among events sharing <paramref name="SequenceNumber"/>.</param>
[Alias("Continuum.Streaming.StreamedEventSequence.V1"), GenerateSerializer, Immutable]
public readonly record struct StreamedEventSequence(BigInteger SequenceNumber, ulong SubSequenceNumber) : IComparable<StreamedEventSequence>
{
    /// <inheritdoc />
    public int CompareTo(StreamedEventSequence other)
    {
        var result = SequenceNumber.CompareTo(other.SequenceNumber);
        return result != 0 ? result : SubSequenceNumber.CompareTo(other.SubSequenceNumber);
    }

    /// <summary>Determines whether <paramref name="left"/> is ordered after <paramref name="right"/>.</summary>
    public static bool operator >(StreamedEventSequence left, StreamedEventSequence right) => left.CompareTo(right) > 0;

    /// <summary>Determines whether <paramref name="left"/> is ordered before <paramref name="right"/>.</summary>
    public static bool operator <(StreamedEventSequence left, StreamedEventSequence right) => left.CompareTo(right) < 0;

    /// <summary>Determines whether <paramref name="left"/> is ordered after or equal to <paramref name="right"/>.</summary>
    public static bool operator >=(StreamedEventSequence left, StreamedEventSequence right) => left.CompareTo(right) >= 0;

    /// <summary>Determines whether <paramref name="left"/> is ordered before or equal to <paramref name="right"/>.</summary>
    public static bool operator <=(StreamedEventSequence left, StreamedEventSequence right) => left.CompareTo(right) <= 0;
}
