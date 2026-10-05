using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class CosmosDBLogConsistentStorageSiloBuilderExtensions
{
    /// <summary>
    ///     Configures CosmosDB as the default log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(this ISiloBuilder builder,
        Action<CosmosDBLogConsistentStorageOptions> configureOptions)
    {
        return builder.AddAzureCosmosDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, 
            configureOptions);
    }

    /// <summary>
    ///     Configures CosmosDB as a log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddAzureCosmosDBBasedLogConsistencyProvider(this ISiloBuilder builder, string name,
        Action<CosmosDBLogConsistentStorageOptions> configureOptions)
    {
        return builder.ConfigureServices(services => services.AddAzureCosmosDBBasedLogConsistencyProvider(name, configureOptions));
    }

    /// <summary>
    ///     Configures CosmosDB as the default log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(this ISiloBuilder builder,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return builder.AddAzureCosmosDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME,
            configureOptions);
    }

    /// <summary>
    ///     Configures CosmosDB as a log consistency storage provider.
    /// </summary>
    public static ISiloBuilder AddAzureCosmosDBBasedLogConsistencyProvider(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return builder.ConfigureServices(services => services.AddAzureCosmosDBBasedLogConsistencyProvider(name, configureOptions));
    }
}
