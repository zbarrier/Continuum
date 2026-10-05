using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBGrainStorageSiloBuilderExtensions
{
    /// <summary>
    ///     Configures KurrentDB as the default grain storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBGrainStorageAsDefault(this ISiloBuilder builder,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        return builder.AddKurrentDBGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a grain storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBGrainStorage(this ISiloBuilder builder, string name,
        Action<KurrentDBGrainStorageOptions> configureOptions)
    {
        return builder.ConfigureServices(services => services.AddKurrentDBGrainStorage(name, configureOptions));
    }

    /// <summary>
    ///     Configures KurrentDB as the default grain storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBGrainStorageAsDefault(this ISiloBuilder builder,
        Action<OptionsBuilder<KurrentDBGrainStorageOptions>>? configureOptions = null)
    {
        return builder.AddKurrentDBGrainStorage(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, configureOptions);
    }

    /// <summary>
    ///     Configures KurrentDB as a grain storage provider.
    /// </summary>
    public static ISiloBuilder AddKurrentDBGrainStorage(this ISiloBuilder builder, string name,
        Action<OptionsBuilder<KurrentDBGrainStorageOptions>>? configureOptions = null)
    {
        return builder.ConfigureServices(services => services.AddKurrentDBGrainStorage(name, configureOptions));
    }
}
