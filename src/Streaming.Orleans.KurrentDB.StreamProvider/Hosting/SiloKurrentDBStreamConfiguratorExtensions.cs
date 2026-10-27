using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Orleans.Hosting;

/// <summary>
///     This class contains extension methods for configuring KurrentDB stream settings in a Silo environment.
/// </summary>
public static class SiloKurrentDBStreamConfiguratorExtensions
{
    /// <summary>
    ///     Configures the cache pressure options for the KurrentDB stream.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the options for the cache pressure.</param>
    public static void ConfigureCachePressuring(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBStreamCachePressureOptions>> configureOptions)
    {
        configurator.Configure(configureOptions);
    }

    /// <summary>
    ///     Configures how the KurrentDB stream cache's block pool is trimmed after demand drops.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the block pool trimming options.</param>
    public static void ConfigureCacheBufferPool(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBStreamCacheBufferPoolOptions>> configureOptions)
    {
        configurator.Configure(configureOptions);
    }
}
