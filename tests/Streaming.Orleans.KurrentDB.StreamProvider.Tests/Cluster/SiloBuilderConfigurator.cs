using System.Text.Json;

using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Orleans.KurrentDB;
using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.TestingHost;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder sb)
    {
        var connectionString = sb.Configuration[ClusterFixture.ConnectionStringKey]
            ?? throw new InvalidOperationException($"{ClusterFixture.ConnectionStringKey} was not provided to the silo.");

        // One client is shared by every connection name. It is registered as an instance so the container does not
        // dispose it once per key; the KurrentDB client throws when disposed a second time.
        var kurrentDBClient = new KurrentDBClient(KurrentDBClientSettings.Create(connectionString));

        // Orleans' binary serializer round-trips the [GenerateSerializer] state types exactly, where the default
        // Newtonsoft serializer depends on every persisted type being Newtonsoft friendly.
        sb.Services.AddOptions<MemoryGrainStorageOptions>("MemoryStorageProvider")
          .PostConfigure<Serializer>((options, serializer) => options.GrainStorageSerializer = new OrleansGrainStorageSerializer(serializer));

        sb.AddMemoryGrainStorage(SubscriptionCheckpointGrain.StorageName);
        sb.Services.AddSubscriptionCheckpointStore<ulong>("checkpointTests");

        sb.AddMemoryGrainStorage("MemoryStorageProvider")
          .AddKurrentDBStreams(Constants.StreamProviderName, options =>
          {
              options.ConnectionName = Constants.StreamProviderName;
              options.Credentials = new KurrentDBStreamCredentialsOptions()
              {
                  UseDefault = true,
              };
              options.Name = Constants.StreamProviderName;
              options.Queues = new List<string>
              {
                  "test-v2-43210",
              };
          })
          // Orleans persists streaming pub/sub subscription state through the store named "PubSubStore".
          // AddKurrentDBPubSubStore bundles the grain storage, Orleans serializer and explicit type mapping
          // that this Orleans internal state type requires.
          .AddKurrentDBPubSubStore(options =>
          {
              options.ConnectionName = Constants.PubSubStoreName;
              options.Credentials = new KurrentDBCredentialsOptions()
              {
                  UseDefault = true,
              };
          })
          .AddKurrentDBBasedLogConsistencyProviderAsDefault(options =>
          {
              options.ConnectionName = "journaledGrainLog";
              options.Credentials = new KurrentDBCredentialsOptions()
              {
                  UseDefault = true,
              };
          });

        // A second provider reading $all. This is the shape the event sourcing storage is consumed through: the
        // events on those streams were not written by a stream provider, so they carry no Orleans metadata.
        sb.AddKurrentDBStreams(Constants.CatchUpProviderName, configurator =>
        {
            configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
            {
                options.ConnectionName = Constants.CatchUpProviderName;
                options.Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true };
                options.Name = Constants.CatchUpProviderName;
                // A $all subscription is global, so this strategy allows exactly one queue.
                options.Queues = new List<string> { Constants.CatchUpQueueName };
            }));
            configurator.UseCatchUpSubscription(ob => ob.Configure(options =>
            {
                options.CheckpointConnectionName = Constants.CatchUpProviderName;
                // Without a filter the subscription would also see the events the other provider and the pub/sub
                // store write to the same container, which are unrelated to what these tests assert on. The prefix
                // is unique per process, so a run only ever reads events it appended itself.
                options.StreamFilterPrefix = Constants.EventSourcedStreamPrefix;
                // Reading the whole log is cheap because the filter scopes it to this run's own events, and it
                // removes any dependence on the subscription being established before the test appends its event.
                options.StartFromNow = false;
            }));
        });

        // A third provider reading $all, used by the implicit subscription tests. It is separate from the provider
        // above because its filter prefix has to be a compile time constant that matches the namespace given to
        // [ImplicitStreamSubscription], whereas the other provider's prefix is generated per run.
        sb.AddKurrentDBStreams(Constants.ImplicitProviderName, configurator =>
        {
            configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
            {
                options.ConnectionName = Constants.ImplicitProviderName;
                options.Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true };
                options.Name = Constants.ImplicitProviderName;
                options.Queues = new List<string> { Constants.ImplicitQueueName };
            }));
            configurator.UseCatchUpSubscription(ob => ob.Configure(options =>
            {
                options.CheckpointConnectionName = Constants.ImplicitProviderName;
                options.StreamFilterPrefix = Constants.ImplicitStreamPrefix;
                options.StartFromNow = false;
            }));
        });

        sb.ConfigureServices(services =>
        {
            var streamClientSettings = KurrentDBClientSettings.Create(connectionString);

            // The stream provider resolves its clients, serializer and type mapper by ConnectionName.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.StreamProviderName, kurrentDBClient);
            // There is no configuration connection string in the test host, so the explicit settings overload is used.
            services.AddKeyedKurrentDBPersistentSubscriptionsClient(Constants.StreamProviderName, streamClientSettings);
            services.AddTypeMappedJsonGrainStorageSerializer(Constants.StreamProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.StreamProviderName, TypeMapKinds.DomainEvent);

            // The $all provider needs the same client, serializer and type mapper so it can convert events written
            // by the event sourcing storage into the Orleans cache format.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.CatchUpProviderName, kurrentDBClient);
            services.AddTypeMappedJsonGrainStorageSerializer(Constants.CatchUpProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.CatchUpProviderName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            // The catch-up strategy owns its read position, so it needs a checkpoint store keyed by the same name.
            services.AddKeyedSingleton<ICheckpointStore<ulong>>(Constants.CatchUpProviderName, (provider, key) =>
            {
                return new KurrentDBCheckpointStore(provider.GetRequiredKeyedService<KurrentDBClient>(Constants.CatchUpProviderName));
            });

            // The implicit subscription provider needs the same set of keyed services.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.ImplicitProviderName, kurrentDBClient);
            services.AddTypeMappedJsonGrainStorageSerializer(Constants.ImplicitProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.ImplicitProviderName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddKeyedSingleton<ICheckpointStore<ulong>>(Constants.ImplicitProviderName, (provider, key) =>
            {
                return new KurrentDBCheckpointStore(provider.GetRequiredKeyedService<KurrentDBClient>(Constants.ImplicitProviderName));
            });

            services.AddKeyedSingleton<KurrentDBClient>("journaledGrainLog", kurrentDBClient);

            // AddKurrentDBPubSubStore registers the storage serializer and type mapper for this store; the
            // keyed client is still supplied by the host.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.PubSubStoreName, kurrentDBClient);

            services.AddSingleton<JsonSerializerOptions>(provider =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                return options;
            });
            services.AddDefaultTypeMapAttributeMappers("journaledGrainLog", TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddTypeMappedJsonGrainStorageSerializer("journaledGrainLog");
        });
    }
}
