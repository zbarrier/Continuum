using Continuum.EventSourcing.Orleans;
using Continuum.EventSourcing.Orleans.CosmosDB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.EventSourcing;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Serialization;

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
        return services.AddAzureCosmosDBBasedLogConsistencyProvider(name, configureOptions, CosmosDBLogConsistentStorageFactory.Create);
    }

    /// <summary>
    ///     Configures Azure Cosmos DB as a log consistency storage provider that routes each grain to the storage for its tenant.
    /// </summary>
    /// <remarks>
    ///     The tenant is read from the grain key, <c>{TenantId}|{Key}</c>. <paramref name="configureTenantOptions"/> is
    ///     called once per tenant with a copy of the provider's options; set <c>ConnectionName</c>, <c>DatabaseName</c>
    ///     or <c>ContainerName</c> to give a tenant dedicated resources. Tenants sharing a connection name share one
    ///     client. Grains without a tenant, and tenants the callback leaves unchanged, use the provider's settings.
    /// </remarks>
    public static IServiceCollection AddMultitenantAzureCosmosDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<string, CosmosDBLogConsistentStorageOptions> configureTenantOptions,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configureTenantOptions);
        return services.AddAzureCosmosDBBasedLogConsistencyProvider(name, configureOptions,
            (sp, key) => CosmosDBLogConsistentStorageFactory.CreateMultitenant(sp, key, configureTenantOptions));
    }

    private static IServiceCollection AddAzureCosmosDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<OptionsBuilder<CosmosDBLogConsistentStorageOptions>>? configureOptions,
        Func<IServiceProvider, string, ILogConsistentStorage> createStorage)
    {
        // Configure log storage. Validation is done by the Orleans IConfigurationValidator below, which runs at silo startup.
        var optionsBuilder = services.AddOptions<CosmosDBLogConsistentStorageOptions>(name)
            .BindConfiguration($"{DefaultConfigSection}:{name}");
        configureOptions?.Invoke(optionsBuilder);
        services.AddTransient<IConfigurationValidator>(sp => new CosmosDBLogConsistentStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<CosmosDBLogConsistentStorageOptions>>().Get(name), name));
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<CosmosDBLogConsistentStorageOptions>, DefaultCosmosDBLogConsistentStorageOptionsConfigurator>());
        services.ConfigureNamedOptionForLogging<CosmosDBLogConsistentStorageOptions>(name);

        services.AddKeyedSingleton<ILogConsistentStorage>(name, (sp, key) =>
        {
            return key is string strKey
                ? createStorage(sp, strKey)
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
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var deepCopier = sp.GetRequiredService<DeepCopier>();
            var siloDetails = sp.GetRequiredService<ILocalSiloDetails>();
            return grainContext => new DefaultProtocolServices(grainContext, loggerFactory, deepCopier, siloDetails);
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
