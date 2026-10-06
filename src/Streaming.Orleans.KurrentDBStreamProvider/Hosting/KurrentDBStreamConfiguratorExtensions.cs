using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBStreamConfiguratorExtensions
{

    /// <summary>
    ///     Extension method to configure the KurrentDB stream provider options.
    /// </summary>
    /// <param name="configurator">The KurrentDB stream configurator.</param>
    /// <param name="configureOptions">The action to configure the options builder.</param>
    public static void ConfigureKurrentDB(this IKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBOptions>> configureOptions)
    {
        configurator.Configure(configureOptions);
    }

    /// <summary>
    ///     Extension method to configure the data adapter used by the KurrentDB stream provider.
    /// </summary>
    /// <param name="configurator">The KurrentDB stream configurator.</param>
    /// <param name="dataAdapterFactory">The factory method to create the data adapter.</param>
    public static void ConfigureDataAdapter(this IKurrentDBStreamConfigurator configurator, Func<IServiceProvider, string, IKurrentDBDataAdapter> dataAdapterFactory)
    {
        configurator.ConfigureComponent(dataAdapterFactory);
    }

}
