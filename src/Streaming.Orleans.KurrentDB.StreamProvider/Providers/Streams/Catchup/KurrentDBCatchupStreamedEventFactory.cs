using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;

using Continuum.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     Builds the <see cref="StreamedEvent{T}" /> carriers a catch-up receiver hands to the queue cache.
/// </summary>
internal static class KurrentDBCatchupStreamedEventFactory
{
    /// <summary>
    ///     Creates a carrier for an event read from a <c>$all</c> subscription.
    /// </summary>
    /// <param name="resolvedEvent">The event as delivered by the subscription.</param>
    /// <param name="sourceConnectionName">The logical connection the event was read from.</param>
    /// <param name="nameParser">The parser for the event's stream name, or <see langword="null" /> for the default.</param>
    /// <remarks>
    ///     The commit position is taken from <see cref="ResolvedEvent.OriginalPosition" /> because that is the position
    ///     of the event as read from <c>$all</c>. A resolved link would otherwise report the position of the linked
    ///     event, which lives at an unrelated place in the log and would resume the subscription from the wrong offset.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> FromAllStream(ResolvedEvent resolvedEvent, string sourceConnectionName, IStreamNameParser? nameParser = null)
    {
        var commitPosition = (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition;
        return KurrentDBStreamedEventFactory.Create(resolvedEvent.Event, sourceConnectionName, commitPosition, nameParser);
    }

    /// <summary>
    ///     Creates a carrier for an event read from a single stream subscription.
    /// </summary>
    /// <param name="resolvedEvent">The event as delivered by the subscription.</param>
    /// <param name="sourceConnectionName">The logical connection the event was read from.</param>
    /// <param name="nameParser">The parser for the event's stream name, or <see langword="null" /> for the default.</param>
    /// <remarks>
    ///     The position is the revision of the event in the subscribed stream, taken from
    ///     <see cref="ResolvedEvent.OriginalEventNumber" />. A resolved link would otherwise report the revision of the
    ///     linked event in its own stream, which is unrelated to the subscribed stream.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> FromStream(ResolvedEvent resolvedEvent, string sourceConnectionName, IStreamNameParser? nameParser = null)
    {
        return KurrentDBStreamedEventFactory.Create(resolvedEvent.Event, sourceConnectionName, resolvedEvent.OriginalEventNumber.ToUInt64(), nameParser);
    }

    /// <summary>
    ///     Creates a carrier for a checkpoint reached by a filtered <c>$all</c> subscription.
    /// </summary>
    /// <param name="commitPosition">The <c>$all</c> commit position the server reported.</param>
    /// <param name="sourceConnectionName">The logical connection the position belongs to.</param>
    /// <param name="marker">The serialized checkpoint marker.</param>
    /// <remarks>
    ///     The marker travels through the cache like any other event, so its position is only checkpointed once it is
    ///     purged, after every event received before it. It belongs to the reserved checkpoint stream, which no
    ///     application subscribes to.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> CheckpointMarker(ulong commitPosition, string sourceConnectionName, KurrentDBCheckpointMarkerData marker)
    {
        var streamName = KurrentDBStreamedEventConversions.CheckpointStreamName;
        return new StreamedEvent<KurrentDBRawEvent>(
            Guid.CreateVersion7(),
            marker.EventType,
            streamName,
            0,
            KurrentDBStreamedEventConversions.ToCursor(sourceConnectionName, streamName, commitPosition),
            DateTime.UtcNow,
            new KurrentDBRawEvent(marker.Data));
    }
}
