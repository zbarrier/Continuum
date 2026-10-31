using System.Diagnostics.CodeAnalysis;

using Continuum.Orleans.Tenancy;
using Continuum.Streams;

namespace Continuum.Orleans.Streams;

/// <summary>
/// Helpers shared by <see cref="IStreamNameParser"/> implementations.
/// </summary>
public static class StreamNameParsing
{
    /// <summary>
    /// The separator between a tenant id and the key in a tenant qualified stream name.
    /// </summary>
    public const char TenantSeparator = TenantKey.Separator;

    /// <summary>
    /// Builds a <see cref="StreamName"/> from a name already split into its category and the remainder, splitting the
    /// remainder into tenant and key on the first <see cref="TenantSeparator"/>.
    /// </summary>
    /// <param name="value">The raw stream name.</param>
    /// <param name="category">The category part of the name.</param>
    /// <param name="remainder">Everything after the category, either <c>key</c> or <c>tenantId|key</c>.</param>
    /// <exception cref="FormatException">The tenant id or key is empty, or the key contains <see cref="TenantSeparator"/>.</exception>
    public static StreamName Create(string value, string category, string remainder)
    {
        var error = TryCreateCore(value, category, remainder, out var result);
        return error is null ? result! : throw new FormatException(error);
    }

    /// <summary>
    /// Attempts to build a <see cref="StreamName"/> as <see cref="Create"/> does, returning <see langword="false"/>
    /// instead of throwing when the tenant id or key is invalid.
    /// </summary>
    /// <param name="value">The raw stream name.</param>
    /// <param name="category">The category part of the name.</param>
    /// <param name="remainder">Everything after the category, either <c>key</c> or <c>tenantId|key</c>.</param>
    /// <param name="result">The stream name, when successful.</param>
    public static bool TryCreate(string value, string category, string remainder, [NotNullWhen(true)] out StreamName? result) =>
        TryCreateCore(value, category, remainder, out result) is null;

    private static string? TryCreateCore(string value, string category, string remainder, out StreamName? result)
    {
        result = null;
        if (TenantKey.TrySplitCore(remainder, out var tenantId, out var key) is not null)
        {
            return $"Stream name '{value}' does not have a valid tenant id and key.";
        }

        result = new StreamName(value, category, tenantId, key);
        return null;
    }
}
