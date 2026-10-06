using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;
using Orleans.Streams;

namespace Orleans.Hosting;

/// <summary>
///     This class contains extension methods for configuring KurrentDB stream settings in a Silo environment.
/// </summary>
public static class SiloKurrentDBStreamConfiguratorExtensions
{
    /// <summary>
    ///     Configures a checkpointer for the KurrentDB stream.
    /// </summary>
    /// <typeparam name="TOptions">The options type for the checkpointer.</typeparam>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="checkpointerFactoryBuilder">A delegate that creates the checkpointer factory.</param>
    /// <param name="configureOptions">An action to configure the options for the checkpointer.</param>
    public static void ConfigureCheckpointer<TOptions>(this ISiloKurrentDBStreamConfigurator configurator, Func<IServiceProvider, string, IStreamQueueCheckpointerFactory> checkpointerFactoryBuilder, Action<OptionsBuilder<TOptions>> configureOptions)
        where TOptions : class, new()
    {
        configurator.ConfigureComponent(checkpointerFactoryBuilder, configureOptions);
    }

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
    ///     Configures KurrentDB as backend storage of checkpointer state.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the options for the checkpointer.</param>
    public static void UseKurrentDBCheckpointer(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBStreamCheckpointerOptions>> configureOptions)
    {
        configurator.ConfigureCheckpointer(KurrentDBCheckpointerFactory.CreateFactory, configureOptions);
        var name = configurator.Name;
        configurator.ConfigureDelegate(services =>
        {
            services.ConfigureNamedOptionForLogging<KurrentDBStreamCheckpointerOptions>(name)
                    .AddTransient<IConfigurationValidator>(sp => new KurrentDBOperationOptionsValidator<KurrentDBStreamCheckpointerOptions>(sp.GetOptionsByName<KurrentDBStreamCheckpointerOptions>(name), name));
        });
    }
}
