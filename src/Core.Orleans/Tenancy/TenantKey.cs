namespace Continuum.Orleans.Tenancy;

/// <summary>
/// Builds and splits tenant qualified Orleans keys in the <c>{TenantId}|{Key}</c> format used by Orleans.Multitenant.
/// </summary>
/// <remarks>
/// <para>
/// Continuum does not reference Orleans.Multitenant because it is not AOT compatible; this type reproduces its key format.
/// A key without a tenant is the plain key.
/// </para>
/// <para>
/// Tenant ids and keys may not contain <see cref="Separator"/>, so <see cref="Create"/> only ever inserts a
/// <see cref="SeparatorsSeparator"/> after the separator when the key within the tenant starts with
/// <see cref="SeparatorsSeparator"/>, exactly as Orleans.Multitenant does.
/// </para>
/// </remarks>
public static class TenantKey
{
    /// <summary>
    /// The separator between the tenant id and the key within the tenant.
    /// </summary>
    public const char Separator = '|';

    /// <summary>
    /// The character inserted after <see cref="Separator"/> when the key within the tenant starts with
    /// <see cref="Separator"/> or <see cref="SeparatorsSeparator"/>.
    /// </summary>
    public const char SeparatorsSeparator = '~';

    /// <summary>
    /// Builds an Orleans key for <paramref name="keyWithinTenant"/>, qualified by <paramref name="tenantId"/> when it is not <see langword="null"/>.
    /// </summary>
    /// <param name="tenantId">The tenant id, or <see langword="null"/> for a key without a tenant.</param>
    /// <param name="keyWithinTenant">The key within the tenant.</param>
    /// <returns><c>{tenantId}|{keyWithinTenant}</c>, or <paramref name="keyWithinTenant"/> when <paramref name="tenantId"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="tenantId"/> is empty or contains <see cref="Separator"/>, or <paramref name="keyWithinTenant"/> is empty or contains <see cref="Separator"/>.
    /// </exception>
    public static string Create(string? tenantId, string keyWithinTenant)
    {
        ValidateKeyWithinTenant(keyWithinTenant);
        if (tenantId is null)
        {
            return keyWithinTenant;
        }

        ValidateTenantId(tenantId);
        return keyWithinTenant[0] == SeparatorsSeparator
            ? $"{tenantId}{Separator}{SeparatorsSeparator}{keyWithinTenant}"
            : $"{tenantId}{Separator}{keyWithinTenant}";
    }

    /// <summary>
    /// Splits an Orleans key into its tenant id and the key within the tenant.
    /// </summary>
    /// <param name="key">The Orleans key, either <c>{TenantId}|{Key}</c> or a plain key.</param>
    /// <param name="tenantId">The tenant id, or <see langword="null"/> when <paramref name="key"/> is not tenant qualified.</param>
    /// <param name="keyWithinTenant">The key within the tenant.</param>
    /// <exception cref="FormatException">
    /// The tenant id or the key within the tenant is empty, or the key within the tenant contains <see cref="Separator"/>.
    /// </exception>
    public static void Split(string key, out string? tenantId, out string keyWithinTenant)
    {
        ArgumentNullException.ThrowIfNull(key);

        var error = TrySplitCore(key, out tenantId, out keyWithinTenant);
        if (error is not null)
        {
            throw new FormatException(error);
        }
    }

    internal static string? TrySplitCore(string key, out string? tenantId, out string keyWithinTenant)
    {
        tenantId = null;
        keyWithinTenant = key;
        var separatorIndex = key.IndexOf(Separator);
        if (separatorIndex < 0)
        {
            return key.Length == 0 ? "Key is empty." : null;
        }

        if (separatorIndex == 0)
        {
            return $"Key '{key}' has an empty tenant id.";
        }

        var keyStart = separatorIndex + 1;
        if (keyStart + 1 < key.Length && key[keyStart] == SeparatorsSeparator && key[keyStart + 1] == SeparatorsSeparator)
        {
            keyStart++;
        }

        if (keyStart == key.Length)
        {
            return $"Key '{key}' has an empty key within the tenant.";
        }

        if (key.IndexOf(Separator, keyStart) >= 0)
        {
            return $"Key '{key}' has a key within the tenant that contains '{Separator}'.";
        }

        tenantId = key[..separatorIndex];
        keyWithinTenant = key[keyStart..];
        return null;
    }

    /// <summary>
    /// Validates that <paramref name="tenantId"/> is non-empty and does not contain <see cref="Separator"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="tenantId"/> is empty or contains <see cref="Separator"/>.</exception>
    public static void ValidateTenantId(string tenantId)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenantId);
        if (tenantId.Contains(Separator))
        {
            throw new ArgumentException($"Tenant id '{tenantId}' must not contain '{Separator}'.", nameof(tenantId));
        }
    }

    /// <summary>
    /// Validates that <paramref name="keyWithinTenant"/> is non-empty and does not contain <see cref="Separator"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="keyWithinTenant"/> is empty or contains <see cref="Separator"/>.</exception>
    public static void ValidateKeyWithinTenant(string keyWithinTenant)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyWithinTenant);
        if (keyWithinTenant.Contains(Separator))
        {
            throw new ArgumentException($"Key '{keyWithinTenant}' must not contain '{Separator}'.", nameof(keyWithinTenant));
        }
    }
}
