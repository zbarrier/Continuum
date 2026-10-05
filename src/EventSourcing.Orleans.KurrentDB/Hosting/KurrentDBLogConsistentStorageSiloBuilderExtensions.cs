using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBLogConsistentStorageSiloBuilderExtensions
{
    /// <summary>
    ///     Configures KurrentDB as the default log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBBasedLogConsistencyProviderAsDefault(this ISiloBuilder builder,
        Action<KurrentDBLogConsistentStorageOptions> configureOptions)
    {
        return builder.AddKurrentDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME,
            configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBBasedLogConsistencyProvider(this ISiloBuilder builder, string name,
        Action<KurrentDBLogConsistentStorageOptions> configureOptions)
    {
        return builder.ConfigureServices(services => services.AddKurrentDBBasedLogConsistencyProvider(name, configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as the default log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBBasedLogConsistencyProviderAsDefault(this ISiloBuilder builder,
        Action<OptionsBuilder<KurrentDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return builder.AddKurrentDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME,
            configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBBasedLogConsistencyProvider(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<KurrentDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return builder.ConfigureServices(services => services.AddKurrentDBBasedLogConsistencyProvider(name, configureOptions));
    }
}
