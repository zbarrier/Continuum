namespace Continuum.Streaming;

/// <summary>
///     Producer supplied metadata describing a streamed event, keyed by name.
/// </summary>
/// <remarks>
///     <para>
///         Metadata is for values a consumer may want to read without deserializing the event, such as routing hints
///         and observability data. Structured data belongs in the event payload, where the type mapper and the serializer
///         already resolve types properly. Values are therefore plain strings and are never interpreted here.
///     </para>
///     <para>
///         Transports reserve names beginning with <see cref="ReservedPrefix" /> for their own use. KurrentDB, for
///         example, writes "$traceId" and "$spanId" into the same metadata document the producer writes to. Reserved
///         names are lifted onto the dedicated properties below when read, and dropped when written, so a producer
///         cannot collide with them.
///     </para>
/// </remarks>
public interface IStreamedEventMetadata : IReadOnlyDictionary<string, string>
{
    /// <summary>
    ///     The prefix transports reserve for metadata they own.
    /// </summary>
    public const string ReservedPrefix = "$";

    /// <summary>
    ///     The identifier of the distributed trace the event was produced within, as a 32 character hex string.
    /// <para>
    ///     This is the W3C trace id, written by the producer at append time, which is what allows a consumer to
    ///     correlate its handling back to the operation that produced the event.
    /// </para>
    /// <para>For KurrentDB, this comes from the "$traceId" entry the client writes into the event metadata.</para>
    /// <para><see langword="null" /> when the event was produced without an active trace.</para>
    /// </summary>
    string? TraceId { get; }

    /// <summary>
    ///     The identifier of the span that produced the event, as a 16 character hex string.
    /// <para>
    ///     Together with <see cref="TraceId" /> this identifies the producing operation, so a consumer can parent its
    ///     own activity to it rather than starting an unrelated trace.
    /// </para>
    /// <para>For KurrentDB, this comes from the "$spanId" entry the client writes into the event metadata.</para>
    /// <para><see langword="null" /> when the event was produced without an active trace.</para>
    /// </summary>
    string? SpanId { get; }

    /// <summary>
    ///     Identifies the transaction the event was appended in.
    /// <para>
    ///     Written by the producer at append time, because the event store cannot distinguish the events of one append
    ///     from another once they are written.
    /// </para>
    /// <para>
    ///     Events are delivered individually in log order rather than as transaction batches. A KurrentDB commit is
    ///     written contiguously to <c>$all</c>, so a consumer reading in order already receives a transaction's events
    ///     together and in order, including when the transaction spans several streams. This exists so that the rare
    ///     consumer needing to defer work until a transaction is complete can group events itself, without imposing
    ///     buffering on consumers that do not care.
    /// </para>
    /// <para>
    ///     <see langword="null" /> when the event carries no transaction metadata, which is the case for any event
    ///     written by a foreign producer.
    /// </para>
    /// </summary>
    NewId? TransactionId { get; }

    /// <summary>
    ///     The total number of events in the transaction, across every stream it wrote to.
    /// <para>
    ///     Greater than <see cref="TransactionPartitionSize" /> when the transaction spans more than one stream.
    ///     <see langword="null" /> when <see cref="TransactionId" /> is.
    /// </para>
    /// </summary>
    int? TransactionSize { get; }

    /// <summary>
    ///     The number of events in the transaction that belong to this event's stream.
    /// <para>
    ///     A consumer that groups events by transaction should count against this rather than
    ///     <see cref="TransactionSize" />, which would never be reached for a transaction spanning several streams.
    ///     <see langword="null" /> when <see cref="TransactionId" /> is.
    /// </para>
    /// </summary>
    int? TransactionPartitionSize { get; }

    /// <summary>
    ///     The zero based position of this event among the transaction's events for this stream.
    /// <para>
    ///     Ordered as the producer appended them, and always less than <see cref="TransactionPartitionSize" />.
    ///     <see langword="null" /> when <see cref="TransactionId" /> is.
    /// </para>
    /// </summary>
    int? TransactionPartitionIndex { get; }

    /// <summary>
    ///     Determines whether a name is reserved for use by a transport.
    /// </summary>
    public static bool IsReservedName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.StartsWith(ReservedPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Gets the value stored under the supplied name, or <see langword="null" /> when it is absent.
    /// </summary>
    string? GetValueOrDefault(string name);
}
