using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Orleans.Hosting;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.TypeMapping;

using GetRDone.Orders.Automations;
using GetRDone.Subscriptions.Worker.Orders;

// The connection name (and Aspire resource name) of the shared KurrentDB database, as in the Silo.
const string KurrentDBConnectionName = "kurrentdb";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// The automations call the silo's grains as a cluster client. The client is registered before the subscriptions, so it
// has connected before they start.
builder.AddKeyedAzureTableServiceClient("clustering");
builder.UseOrleansClient(client => client.AddActivityPropagation());

// The subscriptions read the events from KurrentDB with the same type mapping the silo stores them with, and keep their
// checkpoints there too, since what the automations produce is stored there.
builder.AddKeyedKurrentDBClient(KurrentDBConnectionName);
builder.Services.AddDefaultTypeMapAttributeMappers(KurrentDBConnectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
builder.AddTypeMappedJsonGrainStorageSerializer(KurrentDBConnectionName, requireStoredType: true);
builder.AddKurrentDBCheckpointStore(KurrentDBConnectionName);

builder.Services.AddHealthChecks().AddKurrentDBCatchupSubscriptions();
builder.Services.AddOpenTelemetry().WithMetrics(metrics => metrics.AddMeter(KurrentDBCatchupSubscriptionMetrics.MeterName));

// The order number automations. Each order assigns itself the number it was issued, reacting in the silo.
builder.Services.AddSingleton<OrderNumberIssuer>();
AddSubscription(OrderNumberIssuerSubscription.Info);

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();

void AddSubscription(KurrentDBAllStreamCatchupSubscriptionInfo subscriptionInfo)
{
    builder.Services.Configure<KurrentDBCatchupSubscriptionOptions>(subscriptionInfo.Name, options =>
    {
        options.ConnectionName = KurrentDBConnectionName;
        options.CheckpointConnectionName = KurrentDBConnectionName;
    });
    builder.AddKurrentDBAllStreamCatchupSubscription(subscriptionInfo);
}
