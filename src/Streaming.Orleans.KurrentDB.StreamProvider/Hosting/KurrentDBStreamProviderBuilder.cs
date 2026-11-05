using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;

[assembly: RegisterProvider("KurrentDB", "Streaming", "Silo", typeof(KurrentDBStreamProviderBuilder))]

namespace Orleans.Hosting;

/// <summary>
///     Configures a KurrentDB stream provider from the <c>Orleans:Streaming:{Name}</c> configuration section when its
///     <c>ProviderType</c> is <c>KurrentDB</c>.
/// </summary>
/// <remarks>
///     The section is bound to <see cref="KurrentDBOptions" /> and to the receiver options of the strategy selected by
///     <c>SubscriptionType</c>: <c>Persistent</c> (default), <c>AllStreamCatchup</c>, or <c>StreamCatchup</c>.
/// </remarks>
internal sealed class KurrentDBStreamProviderBuilder : IProviderBuilder<ISiloBuilder>
{
    public void Configure(ISiloBuilder builder, string? name, IConfigurationSection configurationSection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var subscriptionType = configurationSection["SubscriptionType"];
        builder.AddKurrentDBStreams(name, configurator =>
        {
            configurator.ConfigureKurrentDB(ob => ob.Configure(options => BindKurrentDBOptions(configurationSection, options)));
            configurator.Configure<KurrentDBDataAdapterOptions>(ob => ob.Configure(options =>
                options.UnknownEventTypeBehavior = configurationSection.GetValue(nameof(options.UnknownEventTypeBehavior), options.UnknownEventTypeBehavior)));
            if (string.IsNullOrWhiteSpace(subscriptionType) || string.Equals(subscriptionType, "Persistent", StringComparison.OrdinalIgnoreCase))
            {
                configurator.UsePersistentSubscriptions(ob => ob.Configure(options =>
                {
                    options.StartFromNow = configurationSection.GetValue(nameof(options.StartFromNow), options.StartFromNow);
                    options.UpdateExistingSubscription = configurationSection.GetValue(nameof(options.UpdateExistingSubscription), options.UpdateExistingSubscription);
                    options.PrefetchCount = configurationSection.GetValue(nameof(options.PrefetchCount), options.PrefetchCount);
                }));
            }
            else if (string.Equals(subscriptionType, "AllStreamCatchup", StringComparison.OrdinalIgnoreCase))
            {
                configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options => BindCatchupOptions(configurationSection, options)));
            }
            else if (string.Equals(subscriptionType, "StreamCatchup", StringComparison.OrdinalIgnoreCase))
            {
                configurator.UseStreamCatchupSubscription(ob => ob.Configure(options => BindCatchupOptions(configurationSection, options)));
            }
            else
            {
                throw new OrleansConfigurationException(
                    $"KurrentDB stream provider '{name}' has an unknown SubscriptionType '{subscriptionType}'. Valid values are Persistent, AllStreamCatchup, and StreamCatchup.");
            }
        });
    }

    // KurrentDB client types (ClientSettings, PersistentSubscriptionSettings) are not configuration bindable, so only
    // the plain settings are bound; the rest stay available through the code based configuration.
    private static void BindKurrentDBOptions(IConfigurationSection section, KurrentDBOptions options)
    {
        options.ConnectionName = section[nameof(options.ConnectionName)] ?? options.ConnectionName;
        section.GetSection(nameof(options.Credentials)).Bind(options.Credentials);
        var queues = section.GetSection(nameof(options.Queues)).Get<List<string>>();
        if (queues is { Count: > 0 })
        {
            options.Queues = queues;
        }
    }

    private static void BindCatchupOptions(IConfigurationSection section, KurrentDBCatchupSubscriptionReceiverOptions options)
    {
        options.StartFromNow = section.GetValue(nameof(options.StartFromNow), options.StartFromNow);
        options.ResolveLinkTos = section.GetValue(nameof(options.ResolveLinkTos), options.ResolveLinkTos);
        options.BufferCapacityMultiplier = section.GetValue(nameof(options.BufferCapacityMultiplier), options.BufferCapacityMultiplier);
        options.CheckpointConnectionName = section[nameof(options.CheckpointConnectionName)] ?? options.CheckpointConnectionName;
        section.GetSection(nameof(options.CheckpointMonitorOptions)).Bind(options.CheckpointMonitorOptions);
    }
}
