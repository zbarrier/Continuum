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
/// Tenant ids and keys may not contain <see cref="Separator"/>, so Orleans.Multitenant never escapes them.
/// When a key within a tenant starts with <see cref="Separator"/> or <see cref="SeparatorsSeparator"/>, a
/// <see cref="SeparatorsSeparator"/> is inserted after the separator, exactly as Orleans.Multitenant does.
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
    /// <exception cref="FormatException">The tenant id or the key within the tenant is empty.</exception>
    public static void Split(string key, out string? tenantId, out string keyWithinTenant)
    {
        ArgumentNullException.ThrowIfNull(key);

        var separatorIndex = key.IndexOf(Separator);
        if (separatorIndex < 0)
        {
            if (key.Length == 0)
            {
                throw new FormatException("Key is empty.");
            }

            tenantId = null;
            keyWithinTenant = key;
            return;
        }

        if (separatorIndex == 0)
        {
            throw new FormatException($"Key '{key}' has an empty tenant id.");
        }

        var keyStart = separatorIndex + 1;
        if (keyStart + 1 < key.Length && key[keyStart] == SeparatorsSeparator && key[keyStart + 1] is SeparatorsSeparator or Separator)
        {
            keyStart++;
        }

        if (keyStart == key.Length)
        {
            throw new FormatException($"Key '{key}' has an empty key within the tenant.");
        }

        tenantId = key[..separatorIndex];
        keyWithinTenant = key[keyStart..];
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
