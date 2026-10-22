using Orleans.Configuration;

namespace Orleans.Hosting;

/// <summary>
/// </summary>
public static class KurrentDBSiloBuilderExtensions
{
    /// <summary>
    ///     Configure silo to use KurrentDB persistent streams.
    /// </summary>
    public static ISiloBuilder AddKurrentDBStreams(this ISiloBuilder builder, string name, Action<ISiloKurrentDBStreamConfigurator> configure)
    {
        var configurator = new SiloKurrentDBStreamConfigurator(name, configureServicesDelegate => builder.ConfigureServices(configureServicesDelegate));
        configure.Invoke(configurator);
        return builder;
    }

    /// <summary>
    ///     Configure silo to use KurrentDB persistent streams with default check pointer and other settings
    /// </summary>
    /// <remarks>
    ///     This overload selects the persistent subscription strategy. Use the
    ///     <see cref="AddKurrentDBStreams(ISiloBuilder, string, Action{ISiloKurrentDBStreamConfigurator})" /> overload
    ///     with <c>UseAllStreamSubscription</c> to consume <c>$all</c> instead.
    /// </remarks>
    public static ISiloBuilder AddKurrentDBStreams(this ISiloBuilder builder, string name, Action<KurrentDBOptions> configureKurrentDB, Action<KurrentDBStreamCheckpointerOptions> configureDefaultCheckpointer)
    {
        return builder.AddKurrentDBStreams(name,
            configurator =>
            {
                configurator.ConfigureKurrentDB(ob => ob.Configure(configureKurrentDB));
                configurator.UsePersistentSubscriptions();
                configurator.UseKurrentDBCheckpointer(ob => ob.Configure(configureDefaultCheckpointer));
            });
    }
}
