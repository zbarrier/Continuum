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

public static class KurrentDBStreamingBuilderExtensions
{
    const string CatchupSubscriptionsConfigKeyPrefix = "Continuum:Orleans:Streaming:KurrentDB:CatchupSubscriptions";

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
