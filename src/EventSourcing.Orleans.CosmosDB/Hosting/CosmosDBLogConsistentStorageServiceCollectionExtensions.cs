using Continuum.EventSourcing.Orleans.CosmosDB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class CosmosDBLogConsistentStorageServiceCollectionExtensions
{
    private const string DefaultConfigSection = "Orleans:EventSourcing:CosmosDB";

    /// <summary>
    ///     Configures CosmosDB as the default log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(this IServiceCollection services,
        Action<CosmosDBLogConsistentStorageOptions> configureOptions)
    {
        return services.AddAzureCosmosDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, 
            ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures CosmosDB as a log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddAzureCosmosDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<CosmosDBLogConsistentStorageOptions> configureOptions)
    {
        return services.AddAzureCosmosDBBasedLogConsistencyProvider(name, ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures CosmosDB as the default log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(this IServiceCollection services,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return services.AddAzureCosmosDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, 
            configureOptions);
    }

    /// <summary>
    ///     Configures CosmosDB as a log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddAzureCosmosDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        // Configure log storage.
        var optionsBuilder = services.AddOptions<CosmosDBLogConsistentStorageOptions>(name)
            .BindConfiguration($"{DefaultConfigSection}:{name}")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        configureOptions?.Invoke(optionsBuilder);
        services.AddTransient<IConfigurationValidator>(sp => new CosmosDBLogConsistentStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<CosmosDBLogConsistentStorageOptions>>().Get(name), name));
        services.AddTransient<IPostConfigureOptions<CosmosDBLogConsistentStorageOptions>, DefaultCosmosDBLogConsistentStorageOptionsConfigurator>();
        services.ConfigureNamedOptionForLogging<CosmosDBLogConsistentStorageOptions>(name);

        services.AddKeyedSingleton<ILogConsistentStorage>(name, (sp, key) =>
        {
            return key is string strKey
                ? CosmosDBLogConsistentStorageFactory.Create(sp, strKey)
                : throw new ArgumentException($"The value provided for the {nameof(key)} parameter must be a string ({nameof(ILogConsistentStorage)}).", nameof(key));
        });
        if (string.Equals(name, ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, StringComparison.Ordinal))
        {
            services.TryAddSingleton(sp => sp.GetKeyedService<ILogConsistentStorage>(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)!);
        }
        services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp => (ILifecycleParticipant<ISiloLifecycle>)sp.GetRequiredKeyedService<ILogConsistentStorage>(name));

        // Configure log consistency.
        services.TryAddSingleton<Factory<IGrainContext, ILogConsistencyProtocolServices>>(sp =>
        {
            var protocolServicesFactory = ActivatorUtilities.CreateFactory(typeof(DefaultProtocolServices), new[] { typeof(IGrainContext) });
            return grainContext => (ILogConsistencyProtocolServices)protocolServicesFactory(sp, new object[] { grainContext });
        });

        // Configure log view adaptor.
        services.AddKeyedSingleton<ILogViewAdaptorFactory>(name, (sp, n) =>
        {
            return n is string strName
                ? LogConsistencyProviderFactory.Create(sp, strName)
                : throw new ArgumentException($"The value provided for the {nameof(n)} parameter must be a string ({nameof(ILogViewAdaptorFactory)}).", nameof(n));
        });
        if (string.Equals(name, ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME, StringComparison.Ordinal))
        {
            services.TryAddSingleton(sp => sp.GetRequiredKeyedService<ILogViewAdaptorFactory>(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME));
        }
        return services;
    }
}
