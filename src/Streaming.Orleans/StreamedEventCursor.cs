using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// The Orleans-serializable <see cref="IStreamedEventCursor"/>.
/// </summary>
/// <param name="SourceConnectionName">The logical connection the event was read from.</param>
/// <param name="Topic">The topic within <paramref name="SourceConnectionName"/> the event was read from.</param>
/// <param name="Partition">The partition of <paramref name="Topic"/> the event was read from.</param>
/// <param name="SequenceNumber">The position of the event within the partition.</param>
/// <param name="SubSequenceNumber">The order of the event among events sharing <paramref name="SequenceNumber"/>.</param>
[Alias("Continuum.Streaming.StreamedEventCursor.V1"), GenerateSerializer, Immutable]
public sealed record StreamedEventCursor(string SourceConnectionName, string Topic, string Partition, BigInteger SequenceNumber, ulong SubSequenceNumber)
    : IStreamedEventCursor
{
    /// <summary>
    /// Returns <paramref name="cursor"/> if it is already a <see cref="StreamedEventCursor"/>, otherwise a copy of it.
    /// </summary>
    public static StreamedEventCursor From(IStreamedEventCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        return cursor as StreamedEventCursor
            ?? new StreamedEventCursor(cursor.SourceConnectionName, cursor.Topic, cursor.Partition, cursor.SequenceNumber, cursor.SubSequenceNumber);
    }
}
