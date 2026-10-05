using System.Buffers;
using System.Globalization;
using System.Text.Json;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;

/// <summary>
///     Reads and writes the JSON document KurrentDB stores in an event's metadata slot.
/// </summary>
/// <remarks>
///     <para>
///         The metadata slot is shared. The KurrentDB client injects "$traceId" and "$spanId" into it whenever an
///         event is appended while an <see cref="System.Diagnostics.Activity" /> is recording, merging them with
///         whatever the caller supplied. A producer therefore cannot treat the slot as its own, and a consumer cannot
///         treat a non-empty slot as proof of who wrote it.
///     </para>
///     <para>
///         Reading lifts the reserved tracing entries onto their own values and passes everything else through as
///         metadata. Values are surfaced as text: JSON is a structured format, but this contract deliberately does not
///         interpret structure, because nothing here knows what type a nested value was meant to be. Callers that need
///         structure should put it in the event payload, where the type mapper and serde resolve it properly.
///     </para>
/// </remarks>
public static class KurrentDBEventMetadataCodec
{
    /// <summary>
    ///     The metadata name the KurrentDB client writes the W3C trace id to.
    /// </summary>
    public const string TraceIdName = "$traceId";

    /// <summary>
    ///     The metadata name the KurrentDB client writes the W3C span id to.
    /// </summary>
    public const string SpanIdName = "$spanId";

    /// <summary>
    ///     The metadata name the Orleans stream id is written under by the stream provider.
    /// </summary>
    /// <remarks>
    ///     This is an ordinary metadata name rather than a reserved one. The provider appends every stream to a shared
    ///     queue stream, so the Orleans stream id cannot be recovered from the KurrentDB stream name and has to travel
    ///     with the event. Keeping it unreserved means it survives a read/write round trip unchanged and stays visible
    ///     to consumers, and its absence is what identifies an event written by someone else.
    /// </remarks>
    public const string OrleansStreamIdName = "orleansStreamingId";

    /// <summary>
    ///     The metadata name the producing transaction's identifier is written under.
    /// </summary>
    /// <remarks>
    ///     KurrentDB reports the same commit and prepare position for every event of an append, so the grouping cannot
    ///     be recovered from the log and has to be recorded by the producer. These names are unreserved so that they
    ///     survive a read/write round trip and stay readable by anything consuming the log directly.
    /// </remarks>
    public const string TransactionIdName = "transactionId";

    /// <summary>
    ///     The metadata name the transaction's total event count is written under, counting every stream it wrote to.
    /// </summary>
    public const string TransactionSizeName = "transactionSize";

    /// <summary>
    ///     The metadata name the count of the transaction's events for this event's own stream is written under.
    /// </summary>
    public const string TransactionPartitionSizeName = "transactionPartitionSize";

    /// <summary>
    ///     The metadata name this event's ordinal within its stream's part of the transaction is written under.
    /// </summary>
    public const string TransactionPartitionIndexName = "transactionPartitionIndex";

    // Utf8JsonWriter writes straight into an IBufferWriter, avoiding the intermediate copy a MemoryStream needs.
    // A transaction-only document is about 141 bytes and one carrying tracing about 218, so 256 covers both with
    // room for a few extra entries before the buffer has to grow.
    private const int InitialBufferSize = 256;

    /// <summary>
    ///     Reads an event's metadata document.
    /// </summary>
    /// <param name="metadata">The raw metadata bytes as read from KurrentDB.</param>
    /// <returns>
    ///     The producer's entries, with the reserved tracing entries and the transaction grouping lifted onto the
    ///     typed properties of the result. Never <see langword="null" />.
    /// </returns>
    /// <remarks>
    ///     Metadata that is absent, empty, or not a JSON object yields empty metadata rather than throwing. A silo
    ///     normally sees every event on the log, including events written by applications that use the slot for
    ///     something else entirely, and one unreadable document must not stall the queue.
    /// </remarks>
    public static KurrentDBEventMetadata Read(ReadOnlySpan<byte> metadata)
    {
        if (metadata.IsEmpty)
        {
            return KurrentDBEventMetadata.Empty;
        }
        try
        {
            var reader = new Utf8JsonReader(metadata);
            using var document = JsonDocument.ParseValue(ref reader);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return KurrentDBEventMetadata.Empty;
            }
            string? traceId = null;
            string? spanId = null;
            string? transactionId = null;
            string? transactionSize = null;
            string? partitionSize = null;
            string? partitionIndex = null;
            List<KeyValuePair<string, string?>>? values = null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.NameEquals(TraceIdName))
                {
                    traceId = ReadValue(property.Value);
                    continue;
                }
                if (property.NameEquals(SpanIdName))
                {
                    spanId = ReadValue(property.Value);
                    continue;
                }
                // Lifted onto Transaction rather than left in the bag, so the grouping has one representation and
                // consumers are not tempted to re-parse it out of metadata.
                if (property.NameEquals(TransactionIdName))
                {
                    transactionId = ReadValue(property.Value);
                    continue;
                }
                if (property.NameEquals(TransactionSizeName))
                {
                    transactionSize = ReadValue(property.Value);
                    continue;
                }
                if (property.NameEquals(TransactionPartitionSizeName))
                {
                    partitionSize = ReadValue(property.Value);
                    continue;
                }
                if (property.NameEquals(TransactionPartitionIndexName))
                {
                    partitionIndex = ReadValue(property.Value);
                    continue;
                }
                // Create drops the remaining reserved names, so any the client adds later cannot leak into the bag.
                (values ??= []).Add(new KeyValuePair<string, string?>(property.Name, ReadValue(property.Value)));
            }
            var transaction = ReadTransaction(transactionId, transactionSize, partitionSize, partitionIndex);
            return KurrentDBEventMetadata.Create(values, traceId, spanId, transaction);
        }
        catch (JsonException)
        {
            return KurrentDBEventMetadata.Empty;
        }
    }

    /// <summary>
    ///     Builds the transaction grouping from its metadata entries, or <see langword="null" /> when the event does
    ///     not carry a usable one.
    /// </summary>
    /// <remarks>
    ///     A partially written or malformed group is discarded rather than half trusted. Reporting no transaction
    ///     leaves a consumer treating the event as ungrouped, which is the safe reading; surfacing a bad count could
    ///     instead leave a consumer waiting for events that will never arrive.
    /// </remarks>
    private static TransactionInfo? ReadTransaction(string? transactionId, string? transactionSize, string? partitionSize, string? partitionIndex)
    {
        if (transactionId is null || !Guid.TryParse(transactionId, out var guid))
        {
            return null;
        }
        var id = NewId.FromSequentialGuid(guid);
        if (!TryParseCount(transactionSize, out var size) || !TryParseCount(partitionSize, out var partition) || !TryParseCount(partitionIndex, out var index))
        {
            return null;
        }
        // A group that does not describe a consistent shape cannot be assembled, so it is safer to ignore it.
        var transaction = new TransactionInfo(id, size, partition, index);
        return transaction.IsValid ? transaction : null;
    }

    private static bool TryParseCount(string? value, out int result)
        => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out result);

    /// <summary>
    ///     Writes a metadata document for an event being appended, or <see langword="null" /> when there is nothing to write.
    /// </summary>
    /// <param name="metadata">The producer supplied metadata. Reserved names are ignored.</param>
    /// <param name="transaction">
    ///     The transaction the event is being appended in, or <see langword="null" /> to write none. Recorded here
    ///     because the event store cannot distinguish the events of one append from another once they are written.
    /// </param>
    /// <remarks>
    ///     Tracing is deliberately not written here. The client injects it during the append, merging it into this
    ///     document, and it is the only component that knows the activity actually in effect at that moment.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="transaction"/> does not describe a consistent group.</exception>
    public static byte[]? Write(KurrentDBEventMetadata? metadata, TransactionInfo? transaction = null)
    {
        transaction?.EnsureValid(nameof(transaction));
        var hasMetadata = metadata is not null && metadata.Count > 0;
        if (!hasMetadata && transaction is null)
        {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>(InitialBufferSize);
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            if (hasMetadata)
            {
                WriteEntries(writer, metadata!);
            }
            if (transaction is { } info)
            {
                WriteTransaction(writer, info);
            }
            writer.WriteEndObject();
        }
        return buffer.WrittenSpan.ToArray();
    }

    /// <summary>
    ///     Writes a metadata document that round trips everything the metadata carries, including tracing.
    /// </summary>
    /// <param name="metadata">The metadata to write, or <see langword="null" /> when there is none.</param>
    /// <returns>The document bytes, or <see langword="null" /> when there was nothing to write.</returns>
    /// <remarks>
    ///     This is for storing metadata that was already read back from the log, such as when caching an event, where
    ///     the tracing context belongs to the original append and must survive. It is the opposite of
    ///     <see cref="Write(KurrentDBEventMetadata?, TransactionInfo?)" />, which omits tracing because the client
    ///     injects the live activity's context during an append.
    /// </remarks>
    public static byte[]? WriteAll(KurrentDBEventMetadata? metadata)
    {
        if (metadata is null)
        {
            return null;
        }
        // KurrentDBEventMetadata.Create only accepts a complete, valid TransactionInfo, so the four values are either
        // all present or all absent; nothing is defaulted here.
        TransactionInfo? transaction = metadata.TransactionId is { } id &&
            metadata.TransactionSize is { } size &&
            metadata.TransactionPartitionSize is { } partitionSize &&
            metadata.TransactionPartitionIndex is { } partitionIndex
                ? new TransactionInfo(id, size, partitionSize, partitionIndex)
                : null;
        if (metadata.Count == 0 && transaction is null && metadata.TraceId is null && metadata.SpanId is null)
        {
            return null;
        }
        var buffer = new ArrayBufferWriter<byte>(InitialBufferSize);
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            WriteEntries(writer, metadata);
            if (metadata.TraceId is { } traceId)
            {
                writer.WriteString(TraceIdName, traceId);
            }
            if (metadata.SpanId is { } spanId)
            {
                writer.WriteString(SpanIdName, spanId);
            }
            if (transaction is { } info)
            {
                WriteTransaction(writer, info);
            }
            writer.WriteEndObject();
        }
        return buffer.WrittenSpan.ToArray();
    }

    private static void WriteEntries(Utf8JsonWriter writer, KurrentDBEventMetadata metadata)
    {
        foreach (var (name, value) in metadata)
        {
            if (KurrentDBEventMetadata.IsReservedName(name))
            {
                continue;
            }
            writer.WriteString(name, value);
        }
    }

    private static void WriteTransaction(Utf8JsonWriter writer, TransactionInfo info)
    {
        // Written as strings so the sizes read back the same way every other metadata value does. Invariant culture
        // keeps the text identical on every silo regardless of its locale.
        writer.WriteString(TransactionIdName, info.TransactionId.ToSequentialGuid().ToString("D", CultureInfo.InvariantCulture));
        writer.WriteString(TransactionSizeName, info.TransactionSize.ToString(CultureInfo.InvariantCulture));
        writer.WriteString(TransactionPartitionSizeName, info.TransactionPartitionSize.ToString(CultureInfo.InvariantCulture));
        writer.WriteString(TransactionPartitionIndexName, info.TransactionPartitionIndex.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    ///     Surfaces a JSON value as text without interpreting its structure.
    /// </summary>
    /// <remarks>
    ///     Numbers and booleans keep their raw JSON text so they round trip. Objects and arrays keep their raw JSON
    ///     too, so nothing is silently dropped, but they are never parsed into anything. Null becomes a null value,
    ///     which <see cref="KurrentDBEventMetadata.Create(IEnumerable{KeyValuePair{string, string}})" /> then skips.
    /// </remarks>
    private static string? ReadValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => value.GetRawText(),
        };
    }
}
