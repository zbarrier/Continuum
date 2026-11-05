using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

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
    ///     <see cref="KurrentDBOptions.Queues" /> is set to <c>$all</c> when no queues are configured, and otherwise
    ///     must contain exactly <c>$all</c>; see <see cref="KurrentDBAllStreamCatchupSubscriptionReceiverOptions" />.
    ///     <para>
    ///         <see cref="StreamPullingAgentOptions.MaxEventDeliveryTime" /> defaults to
    ///         <see cref="Timeout.InfiniteTimeSpan" />, so an event whose delivery keeps failing is retried until it
    ///         succeeds and is never skipped, purged, or checkpointed past. That stream stops for the failing consumer, and
    ///         the queue stops purging and checkpointing, until the delivery succeeds. Set a finite value through the
    ///         pulling agent configuration to let Orleans give up and move past the event instead.
    ///     </para>
    /// </remarks>
    public static ISiloKurrentDBStreamConfigurator UseAllStreamCatchupSubscription(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBAllStreamCatchupSubscriptionReceiverOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));
        var name = configurator.Name;
        configurator.ConfigureDelegate(services => services.PostConfigure<KurrentDBOptions>(name, options =>
        {
            if (options.Queues is null || options.Queues.Count == 0)
            {
                options.Queues = [KurrentDBStreams.All];
            }
        }));
        return configurator.UseCatchupSubscription(configureOptions, (options, kurrentDBOptions) => new KurrentDBAllStreamCatchupSubscriptionReceiverOptionsValidator(options, kurrentDBOptions, name));
    }

    /// <summary>
    ///     Configures the stream provider to consume KurrentDB through a client checkpointed subscription to a single
    ///     stream, such as <c>$ce-{Category}</c> or a custom projection stream.
    /// </summary>
    /// <param name="configurator">The stream configurator.</param>
    /// <param name="configureOptions">An action to configure the receiver options.</param>
    /// <remarks>
    ///     <see cref="KurrentDBOptions.Queues" /> must contain exactly one entry, the name of the stream to subscribe to.
    ///     It cannot be <c>$all</c>; see <see cref="KurrentDBStreamCatchupSubscriptionReceiverOptions" />.
    ///     <para>
    ///         <see cref="StreamPullingAgentOptions.MaxEventDeliveryTime" /> defaults to
    ///         <see cref="Timeout.InfiniteTimeSpan" />, as for
    ///         <see cref="UseAllStreamCatchupSubscription" />.
    ///     </para>
    /// </remarks>
    public static ISiloKurrentDBStreamConfigurator UseStreamCatchupSubscription(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<KurrentDBStreamCatchupSubscriptionReceiverOptions>>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));
        var name = configurator.Name;
        return configurator.UseCatchupSubscription(configureOptions, (options, kurrentDBOptions) => new KurrentDBStreamCatchupSubscriptionReceiverOptionsValidator(options, kurrentDBOptions, name));
    }

    private static ISiloKurrentDBStreamConfigurator UseCatchupSubscription<TOptions>(this ISiloKurrentDBStreamConfigurator configurator, Action<OptionsBuilder<TOptions>>? configureOptions, Func<TOptions, KurrentDBOptions, IConfigurationValidator> createValidator)
        where TOptions : KurrentDBCatchupSubscriptionReceiverOptions, new()
    {
        var name = configurator.Name;
        configurator.Configure<TOptions>(ob =>
        {
            configureOptions?.Invoke(ob);
            ob.Services.AddTransient<IPostConfigureOptions<TOptions>>(sp => new DefaultKurrentDBCatchupSubscriptionReceiverOptionsConfigurator<TOptions>(sp));
        });
        configurator.ConfigureDelegate(services =>
        {
            // Registered first so any pulling agent configuration supplied by the application still runs after it.
            services.Insert(0, ServiceDescriptor.Singleton<IConfigureOptions<StreamPullingAgentOptions>>(new ConfigureNamedOptions<StreamPullingAgentOptions>(name, options => options.MaxEventDeliveryTime = Timeout.InfiniteTimeSpan)));
            services.ConfigureNamedOptionForLogging<TOptions>(name)
                    .AddTransient<IConfigurationValidator>(sp => createValidator(sp.GetOptionsByName<TOptions>(name), sp.GetOptionsByName<KurrentDBOptions>(name)))
                    .AddTransient<IConfigurationValidator>(sp => new KurrentDBCatchupPullingAgentOptionsValidator(sp.GetOptionsByName<StreamPullingAgentOptions>(name), name))
                    .AddKeyedSingleton<IKurrentDBReceiverFactory>(name, (sp, _) => new KurrentDBCatchupSubscriptionReceiverFactory(sp, name, sp.GetOptionsByName<TOptions>(name)));
        });
        return configurator;
    }
}
