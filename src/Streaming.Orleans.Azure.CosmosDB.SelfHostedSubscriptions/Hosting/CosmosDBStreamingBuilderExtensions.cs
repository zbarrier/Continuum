using Continuum.Streaming;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;
using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Continuum.Orleans.Hosting;

/// <summary>Registers Cosmos DB catch-up subscriptions.</summary>
public static class CosmosDBStreamingBuilderExtensions
{
    const string CatchupSubscriptionsConfigKeyPrefix = "Continuum:Orleans:Streaming:CosmosDB:CatchupSubscriptions";

    /// <summary>Registers a catch-up subscription, binding its options from configuration.</summary>
    public static IHostApplicationBuilder AddCosmosDBCatchupSubscription(this IHostApplicationBuilder builder,
        CosmosDBCatchupSubscriptionInfo subscriptionInfo,
        TypeMapKinds typeMapKinds = TypeMapKinds.DomainEvent | TypeMapKinds.Metadata)
    {
        builder.Services.AddOptions<CosmosDBCatchupSubscriptionOptions>(subscriptionInfo.Name)
            .BindConfiguration($"{CatchupSubscriptionsConfigKeyPrefix}:{subscriptionInfo.Name}")
            .Configure<IServiceProvider>((options, serviceProvider) =>
            {
                options.TypeMapKinds = typeMapKinds;
            });

        builder.Services.AddTransient<IConfigurationValidator>(sp => new CosmosDBCatchupSubscriptionOptionsValidator(sp.GetRequiredService<IOptionsMonitor<CosmosDBCatchupSubscriptionOptions>>().Get(subscriptionInfo.Name), subscriptionInfo));
        builder.Services.AddTransient<IPostConfigureOptions<CosmosDBCatchupSubscriptionOptions>, DefaultCosmosDBCatchupSubscriptionOptionsConfigurator>();
        builder.Services.ConfigureNamedOptionForLogging<CosmosDBCatchupSubscriptionOptions>(subscriptionInfo.Name);

        builder.Services.TryAddSingleton<CosmosDBCatchupSubscriptionStatusRegistry>();
        builder.Services.TryAddSingleton(sp => new CosmosDBCatchupSubscriptionMetrics(
            sp.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>(), sp.GetService<IMeterFactory>()));

        builder.Services.AddSingleton(typeof(IStreamSubscription), subscriptionInfo.Type);

        return builder;
    }
}
