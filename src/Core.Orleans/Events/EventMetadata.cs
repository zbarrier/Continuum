using Continuum.Events;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.Orleans.Events;

/// <inheritdoc cref="IEventMetadata" />
[Alias("Continuum.EventMetadata.V1"), GenerateSerializer, Immutable]
[System.Text.Json.Serialization.JsonConverter(typeof(EventMetadataJsonConverter))]
public sealed class EventMetadata : IEventMetadata
{
    /// <summary>
    ///     Metadata containing no entries.
    /// </summary>
    public static readonly EventMetadata Empty = new(new Dictionary<string, string>(0, StringComparer.Ordinal));

    [Id(0)]
    private readonly Dictionary<string, string> _values;

    private EventMetadata(Dictionary<string, string> values)
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
    public Guid? TransactionId { get; private init; }

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
    public static EventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values)
    {
        return Create(values, null, null, null);
    }

    /// <summary>
    ///     Creates metadata from the supplied entries together with the tracing and transaction values a transport
    ///     lifted out of the same document.
    /// </summary>
    /// <param name="values">The entries to copy. Entries with a null or empty name, or a null value, are skipped.</param>
    /// <param name="traceId">The W3C trace id, or <see langword="null" /> when the event carried no tracing.</param>
    /// <param name="spanId">The W3C span id, or <see langword="null" /> when the event carried no tracing.</param>
    /// <param name="transaction">
    ///     The producing transaction, or <see langword="null" /> when there was none. The grouping is all or nothing, so
    ///     it is passed as one value and must be <see cref="TransactionInfo.IsValid"/>.
    /// </param>
    /// <remarks>
    ///     These are held as typed values rather than entries in the bag. They are reserved names, which the bag drops,
    ///     and a consumer reading them should not have to parse strings back into the shapes they already had.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="transaction"/> does not describe a consistent group.</exception>
    public static EventMetadata Create(IEnumerable<KeyValuePair<string, string?>>? values, string? traceId, string? spanId,
        TransactionInfo? transaction)
    {
        transaction?.EnsureValid(nameof(transaction));
        var copy = new Dictionary<string, string>(StringComparer.Ordinal);
        if (values is not null)
        {
            foreach (var (key, value) in values)
            {
                if (string.IsNullOrEmpty(key) || value is null || IEventMetadata.IsReservedName(key))
                {
                    continue;
                }
                copy[key] = value;
            }
        }
        var hasTracing = traceId is not null || spanId is not null;
        if (copy.Count == 0 && !hasTracing && transaction is null)
        {
            return Empty;
        }
        return new EventMetadata(copy)
        {
            TraceId = traceId,
            SpanId = spanId,
            TransactionId = transaction?.TransactionId,
            TransactionSize = transaction?.TransactionSize,
            TransactionPartitionSize = transaction?.TransactionPartitionSize,
            TransactionPartitionIndex = transaction?.TransactionPartitionIndex,
        };
    }

    /// <summary>
    ///     Gets the transaction the event was appended in, or <see langword="null" /> when it carries none.
    /// </summary>
    public TransactionInfo? Transaction => TransactionId is { } id
        ? new TransactionInfo(id, TransactionSize!.Value, TransactionPartitionSize!.Value, TransactionPartitionIndex!.Value)
        : null;

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

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
