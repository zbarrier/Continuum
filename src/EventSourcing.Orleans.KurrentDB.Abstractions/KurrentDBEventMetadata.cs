namespace Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;

/// <inheritdoc cref="IEventMetadata" />
[Alias("Continuum.EventSourcing.KurrentDBEventMetadata.V1"), GenerateSerializer, Immutable]
public sealed class KurrentDBEventMetadata : EventMetadata
{
    /// <summary>
    ///     Metadata containing no entries.
    /// </summary>
    public const string ReservedPrefix = "$";

    /// <summary>
    ///     Metadata containing no entries.
    /// </summary>
    public static readonly KurrentDBEventMetadata Empty = new(new Dictionary<string, string>(0, StringComparer.Ordinal));

    private KurrentDBEventMetadata(Dictionary<string, string> values)
        : base(values)
    {
    }

    /// <summary>
    ///     Creates metadata from the supplied entries, ignoring any whose name is reserved by a transport.
    /// </summary>
    /// <param name="values">The entries to copy. Entries with a null or empty name, or a null value, are skipped.</param>
    /// <remarks>
    ///     Reserved names are dropped rather than rejected, because this is also the path a transport uses when it
    ///     hands back a metadata document that mixes its own entries with the producer's.
    /// </remarks>
    public static KurrentDBEventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values)
    {
        return Create(values, null, null, null, null, null, null);
    }

    /// <summary>
    ///     Creates metadata from the supplied entries together with the tracing and transaction values a transport
    ///     lifted out of the same document.
    /// </summary>
    /// <param name="values">The entries to copy. Entries with a null or empty name, or a null value, are skipped.</param>
    /// <param name="traceId">The W3C trace id, or <see langword="null" /> when the event carried no tracing.</param>
    /// <param name="spanId">The W3C span id, or <see langword="null" /> when the event carried no tracing.</param>
    /// <param name="transactionId">The producing transaction, or <see langword="null" /> when there was none.</param>
    /// <param name="transactionSize">The transaction's total event count, across every stream it wrote to.</param>
    /// <param name="transactionPartitionSize">The count of the transaction's events for this event's own stream.</param>
    /// <param name="transactionPartitionIndex">This event's ordinal within its stream's part of the transaction.</param>
    /// <remarks>
    ///     These are held as typed values rather than entries in the bag. They are reserved names, which the bag drops,
    ///     and a consumer reading them should not have to parse strings back into the shapes they already had.
    /// </remarks>
    public static KurrentDBEventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values, string? traceId, string? spanId,
        NewId? transactionId, int? transactionSize, int? transactionPartitionSize, int? transactionPartitionIndex)
    {
        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        if (values is not null)
        {
            foreach (var (key, value) in values)
            {
                if (string.IsNullOrEmpty(key) || value is null || IsReservedName(key))
                {
                    continue;
                }
                copy[key] = value;
            }
        }
        var hasTracing = traceId is not null || spanId is not null;
        if (copy.Count == 0 && !hasTracing && transactionId is null)
        {
            return Empty;
        }
        return new KurrentDBEventMetadata(copy)
        {
            TraceId = traceId,
            SpanId = spanId,
            TransactionId = transactionId,
            TransactionSize = transactionSize,
            TransactionPartitionSize = transactionPartitionSize,
            TransactionPartitionIndex = transactionPartitionIndex,
        };
    }

    /// <summary>
    ///     Returns <see langword="true" /> when the name is reserved by a transport, and so should not be used in the metadata bag.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns><see langword="true" /> if the name is reserved; otherwise, <see langword="false" />.</returns>
    public static bool IsReservedName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.StartsWith(ReservedPrefix, StringComparison.Ordinal);
    }
}
