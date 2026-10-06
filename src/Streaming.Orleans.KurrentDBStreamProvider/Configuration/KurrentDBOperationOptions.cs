using Continuum;

using KurrentDB.Client;

namespace Orleans.Configuration;

/// <summary>
///     Common options for the KurrentDB backed operations of the stream provider.
/// </summary>
/// <remarks>
///     Mirrors <see cref="KurrentDBOptions" /> so that every KurrentDB backed component of this provider is configured
///     the same way: a <see cref="ConnectionName" /> that resolves the shared keyed <see cref="KurrentDBClient" />, and
///     credentials expressed through <see cref="KurrentDBStreamCredentialsOptions" />.
/// </remarks>
public class KurrentDBOperationOptions
{
    /// <summary>
    ///     KurrentDB storage policy options. (Like operation timeout setting..)
    /// </summary>
    public KurrentDBPolicyOptions PolicyOptions { get; } = new();

    /// <summary>
    ///     The name of the KurrentDB connection used to resolve the keyed <see cref="KurrentDBClient" />.
    /// </summary>
    /// <remarks>
    ///     Preferred over <see cref="ClientSettings" />. When set, the container owned client registered for this name
    ///     is shared rather than a private client being created and disposed per component.
    /// </remarks>
    [Redact]
    public string? ConnectionName { get; set; }

    /// <summary>
    ///     Settings to be used when configuring the KurrentDB client.
    /// </summary>
    /// <remarks>
    ///     Only used when <see cref="ConnectionName" /> is not set. The resulting client is owned and disposed by the
    ///     component that created it.
    /// </remarks>
    [Redact]
    public KurrentDBClientSettings? ClientSettings { get; set; }

    /// <summary>
    ///     The credentials that have permissions to append and read events.
    ///     <c>UseDefault</c> should be true for insecure localhost connections.
    /// </summary>
    [Redact]
    public KurrentDBStreamCredentialsOptions Credentials { get; set; } = new();
}

/// <summary>
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public class KurrentDBOperationOptionsValidator<TOptions> : IConfigurationValidator
    where TOptions : KurrentDBOperationOptions
{
    /// <summary>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="name"></param>
    public KurrentDBOperationOptionsValidator(TOptions options, string? name = null)
    {
        Options = options;
        Name = name;
    }

    /// <summary>
    /// </summary>
    public TOptions Options { get; }

    /// <summary>
    /// </summary>
    public string? Name { get; }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        // Exactly one client source must be configured, otherwise the state manager has no way to reach KurrentDB and
        // configuring both would make it ambiguous which connection actually wins.
        var hasConnectionName = !string.IsNullOrWhiteSpace(Options.ConnectionName);
        var hasClientSettings = Options.ClientSettings is not null;
        if (hasConnectionName == false && hasClientSettings == false)
        {
            throw new OrleansConfigurationException($"{typeof(TOptions).Name} on stream provider {Name} is invalid. Either {nameof(KurrentDBOperationOptions.ConnectionName)} or {nameof(KurrentDBOperationOptions.ClientSettings)} must be configured.");
        }
        if (Options.Credentials is null)
        {
            throw new OrleansConfigurationException($"{typeof(TOptions).Name} on stream provider {Name} is invalid. {nameof(KurrentDBOperationOptions.Credentials)} is required.");
        }
    }
}
