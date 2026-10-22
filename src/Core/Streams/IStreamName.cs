using System.Diagnostics.CodeAnalysis;

namespace Continuum.Streams;

/// <summary>
/// A stream name together with the parts it was parsed into.
/// </summary>
/// <remarks>
/// The raw <see cref="Value"/> is the identity of the stream; <see cref="Category"/>, <see cref="TenantId"/> and
/// <see cref="Key"/> are what an <see cref="IStreamNameParser"/> read out of it. They are carried together so that a
/// consumer never parses the same name twice and the parts can never disagree with the value they came from.
/// </remarks>
public interface IStreamName
{
    /// <summary>
    /// The raw stream name, exactly as the store reported it.
    /// </summary>
    string Value { get; }

    /// <summary>
    /// The category the stream belongs to. For <c>order-123</c> this is <c>order</c>. Empty when the name carries no
    /// category of its own.
    /// </summary>
    string Category { get; }

    /// <summary>
    /// The tenant the stream belongs to, or <see langword="null"/> when the name is not tenant qualified. For
    /// <c>order-acme|123</c> this is <c>acme</c>.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// The key identifying the individual stream within its <see cref="Category"/>. For <c>order-123</c> and
    /// <c>order-acme|123</c> this is <c>123</c>.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Whether the name is tenant qualified, in which case <see cref="TenantId"/> is not <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(TenantId))]
    bool HasTenant { get; }
}
