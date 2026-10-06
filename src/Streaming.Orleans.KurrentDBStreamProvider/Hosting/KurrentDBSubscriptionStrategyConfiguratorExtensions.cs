using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;

namespace Orleans.Hosting;

/// <summary>
///     Extension methods used to select and configure the subscription strategy of the KurrentDB stream provider.
/// </summary>
/// <remarks>
///     Exactly one strategy should be selected per stream provider. Each method registers only the options and
///     validators belonging to its own strategy, so start up validation never fails because of options that belong
///     to the strategy that was not selected.
/// </remarks>
public static class KurrentDBSubscriptionStrategyConfiguratorExtensions
{
    /// <summary>
    ///     Configures the stream provider to consume KurrentDB through persistent subscriptions (consumer groups).
    ///     This is the default strategy.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the receiver options.</param>
    public static ISiloKurrentDBStreamConfigurator UsePersistentSubscriptions(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBPersistentSubscriptionReceiverOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));
        var name = configurator.Name;
        configurator.Configure<KurrentDBPersistentSubscriptionReceiverOptions>(ob =>
        {
            configureOptions?.Invoke(ob);
        });
        configurator.ConfigureDelegate(services =>
        {
            services.ConfigureNamedOptionForLogging<KurrentDBPersistentSubscriptionReceiverOptions>(name)
                    .AddTransient<IConfigurationValidator>(sp => new KurrentDBPersistentSubscriptionReceiverOptionsValidator(sp.GetOptionsByName<KurrentDBPersistentSubscriptionReceiverOptions>(name), name))
                    .AddKeyedSingleton<IKurrentDBReceiverFactory>(name, (sp, _) => new KurrentDBPersistentSubscriptionReceiverFactory(sp, name));
        });
        return configurator;
    }

    /// <summary>
    ///     Configures the stream provider to consume KurrentDB through a client checkpointed <c>$all</c> subscription.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the receiver options.</param>
    /// <remarks>
    ///     Requires exactly one entry in <see cref="KurrentDBOptions.Queues" />; see
    ///     <see cref="KurrentDBAllStreamReceiverOptions" />.
    /// </remarks>
    public static ISiloKurrentDBStreamConfigurator UseAllStreamSubscription(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBAllStreamReceiverOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));
        var name = configurator.Name;
        configurator.Configure<KurrentDBAllStreamReceiverOptions>(ob =>
        {
            configureOptions?.Invoke(ob);
            ob.Services.AddTransient<IPostConfigureOptions<KurrentDBAllStreamReceiverOptions>, DefaultKurrentDBAllStreamReceiverOptionsConfigurator>();
        });
        configurator.ConfigureDelegate(services =>
        {
            services.ConfigureNamedOptionForLogging<KurrentDBAllStreamReceiverOptions>(name)
                    .AddTransient<IConfigurationValidator>(sp => new KurrentDBAllStreamReceiverOptionsValidator(sp.GetOptionsByName<KurrentDBAllStreamReceiverOptions>(name), sp.GetOptionsByName<KurrentDBOptions>(name), name))
                    .AddKeyedSingleton<IKurrentDBReceiverFactory>(name, (sp, _) => new KurrentDBAllStreamReceiverFactory(sp, name));
        });
        return configurator;
    }
}
