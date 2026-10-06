using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;

namespace Orleans.Hosting;

/// <summary>
///     Configures a persistent stream provider based on KurrentDB for use with Orleans silos.
/// </summary>
public class SiloKurrentDBStreamConfigurator : SiloRecoverableStreamConfigurator, ISiloKurrentDBStreamConfigurator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SiloKurrentDBStreamConfigurator" /> class.
    /// </summary>
    /// <param name="name">The name of the stream provider being configured.</param>
    /// <param name="configureServicesDelegate">A delegate used to configure the services collection.</param>
    public SiloKurrentDBStreamConfigurator(string name, Action<Action<IServiceCollection>> configureServicesDelegate)
        : base(name, configureServicesDelegate, KurrentDBQueueAdapterFactory.Create)
    {
        // The data adapter options are always needed: the adapter has to be able to read events written by the event
        // sourcing storage, which use its serializer and type mapper rather than the Orleans ones.
        ConfigureDelegate(services =>
            {
                services.AddTransient<IPostConfigureOptions<KurrentDBDataAdapterOptions>, DefaultKurrentDBDataAdapterOptionsConfigurator>();
                services.ConfigureNamedOptionForLogging<KurrentDBOptions>(name)
                        .ConfigureNamedOptionForLogging<KurrentDBStreamCachePressureOptions>(name)
                        .ConfigureNamedOptionForLogging<KurrentDBDataAdapterOptions>(name)
                        .AddTransient<IConfigurationValidator>(sp => new KurrentDBOptionsValidator(sp.GetOptionsByName<KurrentDBOptions>(name), name))
                        .AddTransient<IConfigurationValidator>(sp => new KurrentDBDataAdapterOptionsValidator(sp.GetOptionsByName<KurrentDBDataAdapterOptions>(name), name))
                        .AddTransient<IConfigurationValidator>(sp => new KurrentDBStreamPullingAgentOptionsValidator(sp.GetOptionsByName<StreamPullingAgentOptions>(name), name))
                        .AddTransient<IConfigurationValidator>(sp => new StreamCheckpointerConfigurationValidator(sp, name));
            });
    }
}
