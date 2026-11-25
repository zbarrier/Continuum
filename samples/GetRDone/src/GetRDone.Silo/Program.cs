using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Orleans.Hosting;
using Continuum.Streaming.Orleans;
using Continuum.TypeMapping;

using GetRDone;
using GetRDone.Email;
using GetRDone.Orders.Grains;
using GetRDone.Orders.Grains.Notifications;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.ReadModels;

using KurrentDB.Client;

using ManagedCode.Orleans.Identity.Server.Extensions;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

const string CheckpointConnectionName = "checkpoints";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Aspire-provided resources: Azure Table storage for Orleans clustering and KurrentDB for event logs.
builder.AddKeyedAzureTableServiceClient("clustering");
builder.AddKeyedKurrentDBClient(GetRDoneStorage.DefaultConnectionName);

builder.Services.AddDefaultTypeMapAttributeMappers(GetRDoneStorage.DefaultConnectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
builder.AddTypeMappedJsonGrainStorageSerializer(GetRDoneStorage.DefaultConnectionName, requireStoredType: true);

// Every tenant shares the default database for now. Give a tenant its own ConnectionName to move it to a dedicated one.
builder.Services.AddMultitenantKurrentDBBasedLogConsistencyProvider(GetRDoneStorage.EventLogProvider,
    (tenantId, options) => { },
    ob => ob.Configure(options => options.ConnectionName = GetRDoneStorage.DefaultConnectionName));

// The Orleans Identity grain filter evaluates [Authorize] attributes using ASP.NET Core authorization services.
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IOrderPermissions, OrderPermissions>();
builder.Services.AddSingleton<IReadModelSeedData, SampleSeedData>();

// The notifications email over SMTP, which is Mailpit locally (the "mailpit" connection string).
builder.Services.AddSmtpEmailSender();

// The emails link to GetRDone.Web, whose URL the AppHost passes as WEB_BASE_URL.
builder.Services.Configure<NotificationOptions>(options => options.WebBaseUrl = builder.Configuration["WEB_BASE_URL"]);

// Where the order projections' stream provider keeps its $all position. It is grain-backed and held in memory, like
// the projections themselves, so a silo restart loses both and the projections are rebuilt from the start of $all.
// A durable checkpoint with in-memory projections would skip every event before the restart.
builder.AddSubscriptionCheckpointStore<ulong>(CheckpointConnectionName);

// Where the order reactions' stream provider keeps its $all position. A reaction runs a command, so it must never run
// again for an event it already handled: its checkpoint is durable, kept in KurrentDB with what the reactions produce.
builder.AddKurrentDBCheckpointStore(GetRDoneStorage.DefaultConnectionName);

builder.UseOrleans(silo =>
{
    // Default grain storage for read-model grains. Orleans also resolves a grain storage provider when activating
    // journaled grains, even though their events are stored by the KurrentDB log-consistency provider.
    silo.AddMemoryGrainStorageAsDefault();
    //silo.AddKurrentDBBasedLogConsistencyProviderAsDefault();
    silo.AddOrleansIdentity();
    silo.AddActivityPropagation();

    silo.AddMemoryGrainStorage(SubscriptionCheckpointGrain.StorageName);
    // Projection state is written with the Orleans serializer, which round-trips the [GenerateSerializer] state exactly.
    silo.AddMemoryGrainStorage(OrderProjections.StorageName);
    silo.Services.AddOptions<MemoryGrainStorageOptions>(OrderProjections.StorageName)
        .PostConfigure<Serializer>((options, serializer) => options.GrainStorageSerializer = new OrleansGrainStorageSerializer(serializer));

    // Reads the order streams from $all and delivers them, and the per-user client orders streams their events name as
    // stream targets, to the order projections' implicit subscriptions. See OrderProjections.
    silo.AddKurrentDBStreams(OrderProjections.ProviderName, configurator =>
    {
        configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
        {
            options.ConnectionName = GetRDoneStorage.DefaultConnectionName;
            options.Name = OrderProjections.ProviderName;
        }));
        configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options =>
        {
            options.Filter = StreamFilter.Prefix("order-");
            options.StartFromNow = false;
            options.CheckpointConnectionName = CheckpointConnectionName;
        }));
        // The projections only subscribe implicitly, so no pub/sub store is needed.
        configurator.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
    });

    // Reads the order number generator streams from $all and delivers the issued numbers, which name the order's stream
    // as a stream target, to the orders' implicit subscriptions. Its own provider and checkpoint keep the reactions from
    // ever being replayed with the projections. See OrderReactors.
    silo.AddKurrentDBStreams(OrderReactors.ProviderName, configurator =>
    {
        configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
        {
            options.ConnectionName = GetRDoneStorage.DefaultConnectionName;
            options.Name = OrderReactors.ProviderName;
        }));
        configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options =>
        {
            options.Filter = StreamFilter.Prefix("orderNumberGenerator-");
            // The first run starts from now; after that the checkpoint resumes where the reactions left off.
            options.StartFromNow = true;
            options.CheckpointConnectionName = GetRDoneStorage.DefaultConnectionName;
        }));
        // The orders only subscribe implicitly, so no pub/sub store is needed.
        configurator.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
    });

    // Reads the order streams from $all and delivers each order's events to its notifiers' implicit subscriptions, such
    // as the New Order Notifier's. Like the order reactions, it has its own provider and a durable checkpoint, so a
    // notification is never sent again when the projections are rebuilt. See NotificationReactors.
    silo.AddKurrentDBStreams(NotificationReactors.ProviderName, configurator =>
    {
        configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
        {
            options.ConnectionName = GetRDoneStorage.DefaultConnectionName;
            options.Name = NotificationReactors.ProviderName;
        }));
        configurator.UseAllStreamCatchupSubscription(ob => ob.Configure(options =>
        {
            // "order-" doesn't match the order number generator's streams, "orderNumberGenerator-".
            options.Filter = StreamFilter.Prefix("order-");
            // The first run starts from now; after that the checkpoint resumes where the notifications left off.
            options.StartFromNow = true;
            options.CheckpointConnectionName = GetRDoneStorage.DefaultConnectionName;
        }));
        // The notifiers only subscribe implicitly, so no pub/sub store is needed.
        configurator.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
