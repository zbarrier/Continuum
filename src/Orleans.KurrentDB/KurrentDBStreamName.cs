using Continuum.Orleans.Tenancy;

namespace Continuum.Orleans.KurrentDB;

/// <summary>
/// Builds KurrentDB stream names of the form <c>{category}-{orleansKey}</c>.
/// </summary>
/// <remarks>
/// KurrentDB ends a stream's category at the first <c>-</c>, so the category may not contain one. The Orleans key is
/// used as-is, i.e. <c>{TenantId}|{Key}</c> for a tenant qualified key or the plain key otherwise, which keeps
/// <c>$ce-{category}</c> spanning all tenants.
/// </remarks>
public static class KurrentDBStreamName
{
    /// <summary>
    /// The separator between the category and the Orleans key.
    /// </summary>
    public const char CategorySeparator = '-';

    /// <summary>
    /// Builds a KurrentDB stream name from a category and an Orleans key.
    /// </summary>
    /// <param name="category">The category, normally the grain type.</param>
    /// <param name="orleansKey">The Orleans grain or stream key.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="category"/> is empty or contains <see cref="CategorySeparator"/> or <see cref="TenantKey.Separator"/>,
    /// or <paramref name="orleansKey"/> is empty.
    /// </exception>
    public static string Format(string category, string orleansKey)
    {
        ValidateCategory(category);
        ArgumentException.ThrowIfNullOrEmpty(orleansKey);
        return $"{category}{CategorySeparator}{orleansKey}";
    }

    /// <summary>
    /// Validates that <paramref name="category"/> is non-empty and contains neither <see cref="CategorySeparator"/> nor <see cref="TenantKey.Separator"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="category"/> is empty or contains a separator.</exception>
    public static void ValidateCategory(string category)
    {
        ArgumentException.ThrowIfNullOrEmpty(category);
        if (category.Contains(CategorySeparator) || category.Contains(TenantKey.Separator))
        {
            throw new ArgumentException($"Category '{category}' must not contain '{CategorySeparator}' or '{TenantKey.Separator}'.", nameof(category));
        }
    }
}
