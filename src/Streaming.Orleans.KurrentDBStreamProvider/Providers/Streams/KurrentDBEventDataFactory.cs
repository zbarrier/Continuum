using Continuum;
using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;

using KurrentDB.Client;

using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Builds the KurrentDB records a batch of Orleans events is appended as.
/// </summary>
/// <remarks>
///     Each event becomes its own record, the same way the event sourcing storage writes a log entry, rather than the
///     whole batch being packed into one record. Writing the events individually is what makes a record written by
///     this provider readable by anything that reads the log, because the record then carries the mapped event type
///     name and a bare serialized event instead of an envelope only this provider understands.
///
///     The records of one batch are appended together, so they are recorded as a transaction in the same way an
///     append from the event sourcing storage is. That grouping is the only thing that survives to tell a subscriber
///     the events were written as a unit, since KurrentDB reports the same commit position for all of them.
/// </remarks>
internal static class KurrentDBEventDataFactory
{
    /// <summary>
    ///     Converts a batch of events into one record per event.
    /// </summary>
    /// <typeparam name="T">The type the producer supplied the events as.</typeparam>
    /// <param name="streamId">The Orleans stream the events were produced to.</param>
    /// <param name="events">The events to append.</param>
    /// <param name="options">Supplies the type mapper and serializer that define the record format.</param>
    /// <returns>The records to append, in the order the producer supplied them.</returns>
    /// <remarks>
    ///     The Orleans stream id travels as a <see cref="KurrentDBEventMetadataCodec.OrleansStreamIdName" /> entry in
    ///     the JSON metadata document rather than as raw bytes in the metadata slot. The slot is shared: the client
    ///     merges its tracing context into it during the append, so raw bytes written here would be corrupted, and a
    ///     non-empty slot cannot be read back as a bare stream id.
    ///
    ///     The Orleans <c>RequestContext</c> is deliberately not persisted. It is ambient call-chain state belonging
    ///     to the grain call that produced the event, not information about the event, and reentrancy entries in
    ///     particular are meaningless once replayed. Tracing is carried by the trace values the codec lifts on read.
    /// </remarks>
    public static EventData[] ToEventData<T>(StreamId streamId, IEnumerable<T> events, KurrentDBDataAdapterOptions options)
    {
        ArgumentNullException.ThrowIfNull(events, nameof(events));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        var batch = events as IList<T> ?? events.ToList();
        if (batch.Count == 0)
        {
            return [];
        }
        // A batch is produced to a single Orleans stream, so every record belongs to that one stream and the
        // transaction size and the per stream count are the same, exactly as they are for a grain's own append.
        var transactionId = NewId.Next();
        var streamIdText = streamId.ToString();
        var records = new EventData[batch.Count];
        for (var index = 0; index < batch.Count; index++)
        {
            records[index] = ToEventData(batch[index], streamIdText, options,
                new KurrentDBEventMetadataCodec.TransactionInfo(transactionId, batch.Count, batch.Count, index));
        }
        return records;
    }

    /// <summary>
    ///     Converts a single event into the record it is appended as.
    /// </summary>
    private static EventData ToEventData<T>(T @event, string streamId, KurrentDBDataAdapterOptions options, KurrentDBEventMetadataCodec.TransactionInfo transaction)
    {
        // The mapped name rather than the CLR type name, so events can be renamed without breaking readers, and so
        // the reading side resolves the type the same way for both producers.
        var eventType = @event is null ? options.TypeMapper.GetTypeName<T>() : options.TypeMapper.GetTypeName(@event.GetType());
        var metadata = KurrentDBEventMetadataCodec.Write(
            StreamedEventMetadata.Create([new KeyValuePair<string, string?>(KurrentDBEventMetadataCodec.OrleansStreamIdName, streamId)]),
            transaction);
        var data = new ReadOnlyMemory<byte>(options.StreamEventSerde.Serialize(@event));
        return new EventData(Uuid.NewUuid(), eventType, data, metadata, options.ContentType);
    }
}
