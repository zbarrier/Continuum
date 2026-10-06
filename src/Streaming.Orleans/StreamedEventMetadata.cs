using System.Collections;
using System.Diagnostics.CodeAnalysis;

using Continuum.Streaming;

namespace Continuum.Streaming.Orleans;

/// <inheritdoc cref="IStreamedEventMetadata" />
[Alias("Continuum.Streaming.StreamedEventMetadata.V1"), GenerateSerializer, Immutable]
public sealed class StreamedEventMetadata : IStreamedEventMetadata
{
    /// <summary>
    ///     Metadata containing no entries.
    /// </summary>
    public static readonly StreamedEventMetadata Empty = new(new Dictionary<string, string>(0, StringComparer.Ordinal));

    [Id(0)]
    private readonly Dictionary<string, string> _values;

    private StreamedEventMetadata(Dictionary<string, string> values)
    {
        _values = values;
    }

    /// <inheritdoc />
    [Id(1)]
    public string? TraceId { get; private init; }

    /// <inheritdoc />
    [Id(2)]
    public string? SpanId { get; private init; }

    /// <inheritdoc />
    [Id(3)]
    public NewId? TransactionId { get; private init; }

    /// <inheritdoc />
    [Id(4)]
    public int? TransactionSize { get; private init; }

    /// <inheritdoc />
    [Id(5)]
    public int? TransactionPartitionSize { get; private init; }

    /// <inheritdoc />
    [Id(6)]
    public int? TransactionPartitionIndex { get; private init; }

    /// <summary>
    ///     Creates metadata from the supplied entries, ignoring any whose name is reserved by a transport.
    /// </summary>
    /// <param name="values">The entries to copy. Entries with a null or empty name, or a null value, are skipped.</param>
    /// <remarks>
    ///     Reserved names are dropped rather than rejected, because this is also the path a transport uses when it
    ///     hands back a metadata document that mixes its own entries with the producer's.
    /// </remarks>
    public static StreamedEventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values)
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
    public static StreamedEventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values, string? traceId, string? spanId,
        NewId? transactionId, int? transactionSize, int? transactionPartitionSize, int? transactionPartitionIndex)
    {
        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        if (values is not null)
        {
            foreach (var (key, value) in values)
            {
                if (string.IsNullOrEmpty(key) || value is null || IStreamedEventMetadata.IsReservedName(key))
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
        return new StreamedEventMetadata(copy)
        {
            TraceId = traceId,
            SpanId = spanId,
            TransactionId = transactionId,
            TransactionSize = transactionSize,
            TransactionPartitionSize = transactionPartitionSize,
            TransactionPartitionIndex = transactionPartitionIndex,
        };
    }

    /// <inheritdoc />
    public string? GetValueOrDefault(string name) => _values.GetValueOrDefault(name);

    #region IReadOnlyDictionary Implementation

    /// <inheritdoc />
    public string this[string key] => _values[key];

    /// <inheritdoc />
    public IEnumerable<string> Keys => _values.Keys;

    /// <inheritdoc />
    public IEnumerable<string> Values => _values.Values;

    /// <inheritdoc />
    public int Count => _values.Count;

    /// <inheritdoc />
    public bool ContainsKey(string key) => _values.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) => _values.TryGetValue(key, out value);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
