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
    ///     Required. The container owned client registered for this name is shared rather than a private client being
    ///     created per component, so tenants that share a database also share one client.
    /// </remarks>
    [Redact]
    public string? ConnectionName { get; set; }

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
        if (string.IsNullOrWhiteSpace(Options.ConnectionName))
        {
            throw new OrleansConfigurationException($"{typeof(TOptions).Name} on stream provider {Name} is invalid. {nameof(KurrentDBOperationOptions.ConnectionName)} is required.");
        }
        if (Options.Credentials is null)
        {
            throw new OrleansConfigurationException($"{typeof(TOptions).Name} on stream provider {Name} is invalid. {nameof(KurrentDBOperationOptions.Credentials)} is required.");
        }
    }
}
