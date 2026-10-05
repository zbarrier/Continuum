using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers;
using Orleans.Storage;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBGrainStorageServiceCollectionExtensions
{
    /// <summary>
    ///     Configures KurrentDB as the default grain storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBGrainStorageAsDefault(this IServiceCollection services,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        return services.AddKurrentDBGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, 
            ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as a grain storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBGrainStorage(this IServiceCollection services, string name,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        return services.AddKurrentDBGrainStorage(name, ob => ob.Configure(configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as the default grain storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBGrainStorageAsDefault(this IServiceCollection services,
        Action<OptionsBuilder<KurrentDBGrainStorageOptions>>? configureOptions = null)
    {
        return services.AddKurrentDBGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a grain storage provider.
    /// </summary>
    public static IServiceCollection AddKurrentDBGrainStorage(this IServiceCollection services, string name,
        Action<OptionsBuilder<KurrentDBGrainStorageOptions>>? configureOptions = null)
    {
        // Validation is done by the Orleans IConfigurationValidator below, which runs at silo startup.
        var optionsBuilder = services.AddOptions<KurrentDBGrainStorageOptions>(name);
        configureOptions?.Invoke(optionsBuilder);
        services.AddTransient<IConfigurationValidator>(sp => new KurrentDBGrainStorageOptionsValidator(sp.GetRequiredService<IOptionsMonitor<KurrentDBGrainStorageOptions>>().Get(name), name));
        services.AddTransient<IPostConfigureOptions<KurrentDBGrainStorageOptions>, DefaultKurrentDBGrainStorageOptionsConfigurator>();
        services.ConfigureNamedOptionForLogging<KurrentDBGrainStorageOptions>(name);  
        services.AddSingleton(new KurrentDBGrainStorageRegistration(name));
        services.AddKeyedSingleton<IGrainStorage>(name, (sp, key) =>
        {
            return key is string strKey
                ? (IGrainStorage)KurrentDBGrainStorageFactory.Create(sp, strKey)
                : throw new ArgumentException($"The value provided for the {nameof(key)} parameter must be a string ({nameof(IGrainStorage)}).", nameof(key));
        });
        if (string.Equals(name, ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, StringComparison.Ordinal))
        {
            services.TryAddSingleton<IGrainStorage>(sp => sp.GetRequiredKeyedService<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
        }
        services.AddSingleton<ILifecycleParticipant<ISiloLifecycle>>(sp => (ILifecycleParticipant<ISiloLifecycle>)sp.GetRequiredKeyedService<IGrainStorage>(name));
        return services;
    }
}
