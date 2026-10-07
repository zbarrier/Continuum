using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Continuum.Orleans.Hosting;

/// <summary>
///     Registers KurrentDB catch-up subscriptions and checkpoint stores.
/// </summary>
public static class KurrentDBStreamingBuilderExtensions
{
    const string CatchupSubscriptionsConfigKeyPrefix = "Continuum:Orleans:Streaming:KurrentDB:CatchupSubscriptions";

    /// <summary>
    ///     Registers a catch-up subscription as a hosted service, binding its options from configuration.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="subscriptionInfo">The subscription to register.</param>
    /// <param name="typeMapKinds">The kinds of mapped types the subscription resolves events with.</param>
    /// <returns>The host builder.</returns>
    public static IHostApplicationBuilder AddKurrentDBCatchupSubscription(this IHostApplicationBuilder builder,
        KurrentDBCatchupSubscriptionInfo subscriptionInfo,
        TypeMapKinds typeMapKinds = TypeMapKinds.DomainEvent)
    {
        var optionsBuilder = builder.Services.AddOptions<KurrentDBCatchupSubscriptionOptions>(subscriptionInfo.Name)
            .BindConfiguration($"{CatchupSubscriptionsConfigKeyPrefix}:{subscriptionInfo.Name}")
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.TypeMapKinds = typeMapKinds;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddTransient<IConfigurationValidator>(sp => new KurrentDBCatchupSubscriptionOptionsValidator(sp.GetRequiredService<IOptionsMonitor<KurrentDBCatchupSubscriptionOptions>>().Get(subscriptionInfo.Name), subscriptionInfo));
        builder.Services.AddTransient<IPostConfigureOptions<KurrentDBCatchupSubscriptionOptions>, DefaultKurrentDBChannelCatchupSubscriptionOptionsConfigurator>();
        builder.Services.ConfigureNamedOptionForLogging<KurrentDBCatchupSubscriptionOptions>(subscriptionInfo.Name);

        builder.Services.AddSingleton(typeof(IStreamSubscription), subscriptionInfo.Type);

        return builder;
    }



    /// <summary>
    ///     Registers a <see cref="KurrentDBCheckpointStore" /> keyed by a KurrentDB connection name.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="connectionName">The keyed KurrentDB client connection name.</param>
    /// <returns>The host builder.</returns>
    public static IHostApplicationBuilder AddKurrentDBCheckpointStore(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.Services.AddKeyedSingleton<ICheckpointStore<ulong>>(connectionName, (provider, key) =>
        {
            var client = provider.GetRequiredKeyedService<KurrentDBClient>(key);
            return new KurrentDBCheckpointStore(client);
        });

        return builder;
    }
}
