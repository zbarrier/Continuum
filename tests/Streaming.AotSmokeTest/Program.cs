using Continuum.Orleans.Hosting;
using Continuum.Streaming;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;

using KurrentDB.Client;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

// Exercises the Continuum.Streaming libraries under Native AOT.
//   dotnet publish tests/Streaming.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.
// Orleans DeepCopier/serializer setup is not Native AOT compatible upstream, so no silo is started and no
// subscription connects to a database; the publish itself verifies the library code is free of trim/AOT warnings.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// Catch-up subscription registration. Options resolution needs Orleans silo services (serializer, type mapper),
// so only the registrations and diagnostics are checked here.
var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings());
builder.AddKurrentDBCatchupSubscription(SmokeKurrentDBCatchupSubscription.Info);
builder.AddCosmosDBCatchupSubscription(SmokeCosmosDBCatchupSubscription.Info);
builder.Services.AddHealthChecks().AddKurrentDBCatchupSubscriptions().AddCosmosDBCatchupSubscriptions();
Check("KurrentDB catch-up subscription registered", builder.Services.Any(s => s.ImplementationType == typeof(SmokeKurrentDBCatchupSubscription)));
Check("CosmosDB catch-up subscription registered", builder.Services.Any(s => s.ImplementationType == typeof(SmokeCosmosDBCatchupSubscription)));
Check("KurrentDB options validator registered", builder.Services.Any(s => s.ServiceType == typeof(IPostConfigureOptions<KurrentDBCatchupSubscriptionOptions>)));
Check("CosmosDB options validator registered", builder.Services.Any(s => s.ServiceType == typeof(IPostConfigureOptions<CosmosDBCatchupSubscriptionOptions>)));

// Diagnostics resolve and report without reflection.
using (var provider = builder.Services.BuildServiceProvider())
{
    var kurrentRegistry = provider.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>();
    kurrentRegistry.Report(new(SmokeKurrentDBCatchupSubscription.Name, KurrentDBCatchupSubscriptionState.Running));
    Check("KurrentDB metrics resolve", provider.GetRequiredService<KurrentDBCatchupSubscriptionMetrics>() is not null);
    Check("KurrentDB status reported", kurrentRegistry.TryGetStatus(SmokeKurrentDBCatchupSubscription.Name, out _));

    var cosmosRegistry = provider.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>();
    cosmosRegistry.Report(new(SmokeCosmosDBCatchupSubscription.Name, CosmosDBCatchupSubscriptionState.Running));
    Check("CosmosDB metrics resolve", provider.GetRequiredService<CosmosDBCatchupSubscriptionMetrics>() is not null);
    Check("CosmosDB status reported", cosmosRegistry.TryGetStatus(SmokeCosmosDBCatchupSubscription.Name, out _));
}

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

internal sealed class SmokeKurrentDBCatchupSubscription(IServiceProvider serviceProvider)
    : KurrentDBCatchupSubscription<SmokeKurrentDBCatchupSubscription, KurrentDBCatchupSubscriptionOptions>(serviceProvider)
{
    public override string StreamName => "smoke";

    public override IEventFilter? FilterOptions => null;

    protected override Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents) => Task.CompletedTask;
}

internal sealed class SmokeCosmosDBCatchupSubscription(IServiceProvider serviceProvider)
    : CosmosDBCatchupSubscription<SmokeCosmosDBCatchupSubscription, CosmosDBCatchupSubscriptionOptions>(serviceProvider)
{
    protected override Task HandleStreamEventsAsync(ChangeFeedProcessorContext context, List<IStreamedEvent<object>> streamEvents,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
