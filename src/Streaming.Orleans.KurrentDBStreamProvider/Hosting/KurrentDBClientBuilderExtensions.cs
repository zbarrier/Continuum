using Orleans.Configuration;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBClientBuilderExtensions
{
    /// <summary>
    ///     Configure cluster client to use KurrentDB persistent streams.
    /// </summary>
    public static IClientBuilder AddKurrentDBStreams(this IClientBuilder builder, string name, Action<IClusterClientKurrentDBStreamConfigurator> configure)
    {
        var configurator = new ClusterClientKurrentDBStreamConfigurator(name, builder);
        configure.Invoke(configurator);
        return builder;
    }

    /// <summary>
    ///     Configure cluster client to use KurrentDB persistent streams with default settings.
    /// </summary>
    public static IClientBuilder AddKurrentDBStreams(this IClientBuilder builder, string name, Action<KurrentDBOptions> configureKurrentDB)
    {
        builder.AddKurrentDBStreams(name,
            configurator =>
            {
                configurator.ConfigureKurrentDB(ob => ob.Configure(configureKurrentDB));
            });
        return builder;
    }
}
