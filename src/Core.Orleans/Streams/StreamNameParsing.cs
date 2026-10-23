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
    /// <exception cref="FormatException">The tenant id or key is empty.</exception>
    public static StreamName Create(string value, string category, string remainder)
    {
        var separatorIndex = remainder.IndexOf(TenantSeparator);
        if (separatorIndex < 0)
        {
            if (remainder.Length == 0)
            {
                throw new FormatException($"Stream name '{value}' has an empty key.");
            }

            return new StreamName(value, category, null, remainder);
        }

        if (separatorIndex == 0)
        {
            throw new FormatException($"Stream name '{value}' has an empty tenant id.");
        }

        if (separatorIndex == remainder.Length - 1)
        {
            throw new FormatException($"Stream name '{value}' has an empty key.");
        }

        TenantKey.Split(remainder, out var tenantId, out var key);
        return new StreamName(value, category, tenantId, key);
    }
}
