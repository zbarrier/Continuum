using Continuum.Orleans.KurrentDB;
using Continuum;
using Continuum.Streaming;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;

using KurrentDB.Client;
using Continuum.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

/// <summary>
///     Builds the <see cref="StreamedEvent{T}" /> carrier the receivers hand to the queue cache.
/// </summary>
internal static class KurrentDBStreamedEventFactory
{
    private static readonly IStreamNameParser DefaultNameParser = new DefaultKurrentDBStreamNameParser();

    /// <summary>
    ///     Creates a carrier for an event record. Each strategy decides which commit position the event carries.
    /// </summary>
    internal static StreamedEvent<KurrentDBRawEvent> Create(EventRecord record, string sourceConnectionName, ulong commitPosition, IStreamNameParser? nameParser)
    {
        var payload = new KurrentDBRawEvent(record.Data.ToArray());

        // The metadata slot is shared. Whoever appended the event may have supplied entries of their own, and the
        // client merges its tracing context into the same document whenever an Activity was recording. Decoding it
        // here means the carrier says what the event actually carried, rather than the consumer inferring it from
        // whether the slot happened to be occupied.
        var metadata = KurrentDBEventMetadataCodec.Read(record.Metadata.Span);

        // Events reach this provider from two writers. Those appended by the provider itself record their Orleans
        // stream id in metadata and their KurrentDB name carries no category, while events appended by event
        // sourcing are named "category-key". Parsing is therefore attempted rather than assumed: a name that does
        // not split falls back to itself as the key, so a stream that carries no category is still identified.
        if (!(nameParser ?? DefaultNameParser).TryParse(record.EventStreamId, out var streamName))
        {
            // Not a "category-key" name. The whole name is the key.
            streamName = new Continuum.Orleans.Streams.StreamName(record.EventStreamId, string.Empty, null, record.EventStreamId);
        }

        return new StreamedEvent<KurrentDBRawEvent>(
            record.EventId.ToGuid(),
            record.EventType,
            streamName,
            record.EventNumber,
            KurrentDBStreamedEventConversions.ToCursor(sourceConnectionName, streamName, commitPosition),
            record.Created,
            payload,
            metadata);
    }
}
