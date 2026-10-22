using System.Diagnostics.CodeAnalysis;

using Continuum.Streams;

namespace Continuum.Orleans.Streams;

/// <summary>
/// The Orleans-serializable <see cref="IStreamName"/>.
/// </summary>
/// <remarks>
/// Equality is by <see cref="Value"/> only, because the parts are derived from it.
/// </remarks>
[Alias("Continuum.StreamName.V1"), GenerateSerializer, Immutable]
public sealed class StreamName : IStreamName, IEquatable<StreamName>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamName"/> class.
    /// </summary>
    /// <param name="value">The raw stream name.</param>
    /// <param name="category">The category the stream belongs to, or empty when the name carries none.</param>
    /// <param name="tenantId">The tenant the stream belongs to, or <see langword="null"/> when it is not tenant qualified.</param>
    /// <param name="key">The key identifying the individual stream within its category.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> or <paramref name="key"/> is empty, or <paramref name="tenantId"/> is empty.</exception>
    public StreamName(string value, string category, string? tenantId, string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        ArgumentNullException.ThrowIfNull(category);
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (tenantId is not null)
        {
            ArgumentException.ThrowIfNullOrEmpty(tenantId);
        }

        Value = value;
        Category = category;
        TenantId = tenantId;
        Key = key;
    }

    /// <inheritdoc />
    [Id(0)]
    public string Value { get; }

    /// <inheritdoc />
    [Id(1)]
    public string Category { get; }

    /// <inheritdoc />
    [Id(2)]
    public string? TenantId { get; }

    /// <inheritdoc />
    [Id(3)]
    public string Key { get; }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(TenantId))]
    public bool HasTenant => TenantId is not null;

    /// <summary>
    /// Returns <paramref name="streamName"/> if it is already a <see cref="StreamName"/>, otherwise a copy of it.
    /// </summary>
    public static StreamName From(IStreamName streamName)
    {
        ArgumentNullException.ThrowIfNull(streamName);
        return streamName as StreamName ?? new StreamName(streamName.Value, streamName.Category, streamName.TenantId, streamName.Key);
    }

    /// <inheritdoc />
    public bool Equals(StreamName? other) => other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is StreamName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Returns the raw stream name.</summary>
    public override string ToString() => Value;
}
