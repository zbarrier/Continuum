using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;

namespace Orleans.Hosting;

/// <summary>
///     Configuration object for a cluster client using the KurrentDB stream provider.
/// </summary>
public class ClusterClientKurrentDBStreamConfigurator : ClusterClientPersistentStreamConfigurator, IClusterClientKurrentDBStreamConfigurator
{
    /// <summary>
    ///     Constructs a new <see cref="ClusterClientKurrentDBStreamConfigurator" /> object.
    /// </summary>
    /// <param name="name">The name of the stream provider to configure.</param>
    /// <param name="builder">The client builder.</param>
    public ClusterClientKurrentDBStreamConfigurator(string name, IClientBuilder builder)
        : base(name, builder, KurrentDBQueueAdapterFactory.Create)
    {
        builder.ConfigureServices(services =>
        {
            services.ConfigureNamedOptionForLogging<KurrentDBOptions>(name)
                    .AddTransient<IConfigurationValidator>(sp => new KurrentDBOptionsValidator(sp.GetOptionsByName<KurrentDBOptions>(name), name));
        });
    }
}
