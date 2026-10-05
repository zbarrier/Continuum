using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.EventSourcing.Orleans;

/// <inheritdoc cref="IEventMetadata" />
// No alias: an abstract type is never the runtime type of a serialized value, so its subclasses' aliases are the ones written.
[GenerateSerializer, Immutable]
public abstract class EventMetadata : IEventMetadata
{
    [Id(0)]
    private readonly Dictionary<string, string> _values;

    /// <summary>
    ///     Initializes the metadata with the entries it exposes.
    /// </summary>
    /// <param name="values">The entries, already filtered by the derived type. The dictionary is taken over, not copied.</param>
    protected EventMetadata(Dictionary<string, string> values)
    {
        _values = values;
    }

    /// <inheritdoc />
    [Id(1)]
    public string? TraceId { get; protected init; }

    /// <inheritdoc />
    [Id(2)]
    public string? SpanId { get; protected init; }

    /// <inheritdoc />
    [Id(3)]
    public NewId? TransactionId { get; protected init; }

    /// <inheritdoc />
    [Id(4)]
    public int? TransactionSize { get; protected init; }

    /// <inheritdoc />
    [Id(5)]
    public int? TransactionPartitionSize { get; protected init; }

    /// <inheritdoc />
    [Id(6)]
    public int? TransactionPartitionIndex { get; protected init; }


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
