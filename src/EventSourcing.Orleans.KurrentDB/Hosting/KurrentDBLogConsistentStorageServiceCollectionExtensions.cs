using Continuum.EventSourcing.Orleans.KurrentDB;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBLogConsistentStorageServiceCollectionExtensions
{
    private const string DefaultConfigSection = "Orleans:EventSourcing:KurrentDB";

    /// <summary>
    ///     Configures KurrentDB as the default log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBBasedLogConsistencyProviderAsDefault(this IServiceCollection services,
        Action<KurrentDBLogConsistentStorageOptions> configureOptions)
    {
        return services.AddKurrentDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME,
            ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as a log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<KurrentDBLogConsistentStorageOptions> configureOptions)
    {
        return services.AddKurrentDBBasedLogConsistencyProvider(name, ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as the default log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBBasedLogConsistencyProviderAsDefault(this IServiceCollection services,
        Action<OptionsBuilder<KurrentDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        return services.AddKurrentDBBasedLogConsistencyProvider(ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME,
            configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a log consistency storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBBasedLogConsistencyProvider(this IServiceCollection services, string name,
        Action<OptionsBuilder<KurrentDBLogConsistentStorageOptions>>? configureOptions = null)
    {
        // Configure log storage.
        var optionsBuilder = services.AddOptions<KurrentDBLogConsistentStorageOptions>(name)
            .BindConfiguration($"{DefaultConfigSection}:{name}")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        configureOptions?.Invoke(optionsBuilder);
        services.AddTransient<IConfigurationValidator>(sp => new KurrentDBLogConsistentStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<KurrentDBLogConsistentStorageOptions>>().Get(name), name));
        services.AddTransient<IPostConfigureOptions<KurrentDBLogConsistentStorageOptions>, DefaultKurrentDBLogConsistentStorageOptionsConfigurator>();
        services.ConfigureNamedOptionForLogging<KurrentDBLogConsistentStorageOptions>(name);

        services.AddKeyedSingleton<ILogConsistentStorage>(name, (sp, key) =>
        {
            return key is string strKey
                ? KurrentDBLogConsistentStorageFactory.Create(sp, strKey)
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

