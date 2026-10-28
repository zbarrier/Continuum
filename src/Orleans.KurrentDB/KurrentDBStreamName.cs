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
    /// Reserved: the KurrentDB stream provider qualifies delivered namespaces as <c>{provider}:{category}</c>.
    /// </summary>
    public const char NamespaceSeparator = ':';

    /// <summary>
    /// Builds a KurrentDB stream name from a category and an Orleans key.
    /// </summary>
    /// <param name="category">The category, normally the grain type.</param>
    /// <param name="orleansKey">The Orleans grain or stream key.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="category"/> is empty or contains <see cref="CategorySeparator"/>, <see cref="TenantKey.Separator"/> or <see cref="NamespaceSeparator"/>,
    /// or <paramref name="orleansKey"/> is empty.
    /// </exception>
    public static string Format(string category, string orleansKey)
    {
        ValidateCategory(category);
        ArgumentException.ThrowIfNullOrEmpty(orleansKey);
        return $"{category}{CategorySeparator}{orleansKey}";
    }

    /// <summary>
    /// Validates that <paramref name="category"/> is non-empty and contains none of <see cref="CategorySeparator"/>, <see cref="TenantKey.Separator"/> or <see cref="NamespaceSeparator"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="category"/> is empty or contains a separator.</exception>
    public static void ValidateCategory(string category)
    {
        ArgumentException.ThrowIfNullOrEmpty(category);
        if (category.Contains(CategorySeparator) || category.Contains(TenantKey.Separator) || category.Contains(NamespaceSeparator))
        {
            throw new ArgumentException($"Category '{category}' must not contain '{CategorySeparator}', '{TenantKey.Separator}' or '{NamespaceSeparator}'.", nameof(category));
        }
    }
}
