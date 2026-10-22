namespace Continuum.Streaming.Orleans;

/// <summary>
/// The highest position applied by a projection, per topic and partition.
/// </summary>
/// <remarks>
/// <para>
///     Sequence numbers are only ordered within a partition of a topic, so one watermark is kept for each. Every
///     event is delivered from exactly one partition, so an event at or below its partition's watermark has already
///     been applied and is a redelivery.
/// </para>
/// <para>
///     The partition is part of the key because brokers only order sequence numbers within a partition. If a topic
///     is repartitioned, a stream can move to a partition whose sequence numbers are lower than those already
///     applied from its previous partition; a watermark shared across partitions would then discard the new
///     partition's events.
/// </para>
/// </remarks>
[Alias("Continuum.Streaming.ProjectionWatermarks.V1"), GenerateSerializer]
public sealed class ProjectionWatermarks
{
    [Id(0)]
    private readonly Dictionary<TopicPartition, StreamedEventSequence> _sequences = [];

    /// <summary>
    /// The number of topic partitions with a watermark.
    /// </summary>
    public int Count => _sequences.Count;

    /// <summary>
    /// Gets the watermark for a topic partition.
    /// </summary>
    /// <returns>True if anything has been applied from the topic partition.</returns>
    public bool TryGet(string topic, string partition, out StreamedEventSequence sequence)
        => _sequences.TryGetValue(new TopicPartition(topic, partition), out sequence);

    /// <summary>
    /// Determines whether the event at <paramref name="cursor"/> has already been applied.
    /// </summary>
    public bool IsApplied(IStreamedEventCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        return TryGet(cursor.Topic, cursor.Partition, out var last) && ToSequence(cursor) <= last;
    }

    /// <summary>
    /// Records the event at <paramref name="cursor"/> as applied, advancing its partition's watermark if the event
    /// is ahead of it.
    /// </summary>
    /// <returns>True if the watermark moved.</returns>
    public bool Advance(IStreamedEventCursor cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        var key = new TopicPartition(cursor.Topic, cursor.Partition);
        var sequence = ToSequence(cursor);
        if (_sequences.TryGetValue(key, out var last) && sequence <= last)
        {
            return false;
        }

        _sequences[key] = sequence;
        return true;
    }

    private static StreamedEventSequence ToSequence(IStreamedEventCursor cursor)
        => new(cursor.SequenceNumber, cursor.SubSequenceNumber);

    /// <summary>
    /// The scope a watermark applies to.
    /// </summary>
    /// <param name="Topic">The source topic.</param>
    /// <param name="Partition">The partition of <paramref name="Topic"/>.</param>
    [Alias("Continuum.Streaming.ProjectionWatermarks.TopicPartition.V1"), GenerateSerializer, Immutable]
    internal readonly record struct TopicPartition(string Topic, string Partition);
}
