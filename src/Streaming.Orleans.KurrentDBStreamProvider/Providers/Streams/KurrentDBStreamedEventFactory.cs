using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Builds the <see cref="StreamedEvent{T}" /> carrier the receivers hand to the queue cache.
/// </summary>
internal static class KurrentDBStreamedEventFactory
{
    private static readonly IStreamedNameParser NameParser = new DefaultKurrentDBStreamNameParser();

    /// <summary>
    ///     Creates a carrier for an event read from a <c>$all</c> subscription.
    /// </summary>
    /// <param name="resolvedEvent">The event as delivered by the subscription.</param>
    /// <remarks>
    ///     The commit position is taken from <see cref="ResolvedEvent.OriginalPosition" /> because that is the position
    ///     of the event in <c>$all</c>. With link resolution enabled <c>Event.Position</c> points at the linked to
    ///     event, which lives at an unrelated place in the log and would resume the subscription from the wrong offset.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> FromAllStream(ResolvedEvent resolvedEvent)
    {
        var commitPosition = (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition;
        return Create(resolvedEvent.Event, StreamEventExtensions.AllStreamTopic, resolvedEvent.OriginalEventNumber.ToUInt64(), commitPosition);
    }

    /// <summary>
    ///     Creates a carrier for an event delivered by a persistent subscription.
    /// </summary>
    /// <param name="resolvedEvent">The event as delivered by the subscription.</param>
    /// <remarks>
    ///     A persistent subscription tracks progress on the server, so the commit position is only informational here.
    ///     It is still populated when the server reports one so the carrier means the same thing for both strategies.
    /// </remarks>
    public static StreamedEvent<KurrentDBRawEvent> FromPersistentSubscription(ResolvedEvent resolvedEvent)
    {
        var commitPosition = (resolvedEvent.OriginalPosition ?? resolvedEvent.Event.Position).CommitPosition;
        return Create(resolvedEvent.Event, StreamEventExtensions.DefaultTopic, resolvedEvent.OriginalEventNumber.ToUInt64(), commitPosition);
    }

    private static StreamedEvent<KurrentDBRawEvent> Create(EventRecord record, string topic, ulong streamPosition, ulong commitPosition)
    {
        var payload = new KurrentDBRawEvent(record.Data.ToArray(), record.Metadata.ToArray());

        // The metadata slot is shared. Whoever appended the event may have supplied entries of their own, and the
        // client merges its tracing context into the same document whenever an Activity was recording. Decoding it
        // here means the carrier says what the event actually carried, rather than the consumer inferring it from
        // whether the slot happened to be occupied.
        var metadata = KurrentDBEventMetadataCodec.Read(record.Metadata.Span);

        // Events reach this provider from two writers. Those appended by the provider itself record their Orleans
        // stream id in metadata and their KurrentDB name carries no category, while events appended by event
        // sourcing are named "category-key". Parsing is therefore attempted rather than assumed: a name that does
        // not split falls back to itself as the key, so a stream that carries no category is still identified.
        var streamKey = record.EventStreamId;
        try
        {
            streamKey = NameParser.Parse(record.EventStreamId).StreamKey;
        }
        catch (FormatException)
        {
            // Not a "category-key" name. The whole name is the key.
        }

        return new StreamedEvent<KurrentDBRawEvent>(
            NewId.FromSequentialGuid(record.EventId.ToGuid()),
            record.EventType,
            record.EventStreamId,
            streamKey,
            topic,
            StreamEventExtensions.DefaultPartitionId,
            record.EventNumber,
            streamPosition,
            commitPosition,
            0,
            record.Created,
            payload,
            metadata);
    }
}
