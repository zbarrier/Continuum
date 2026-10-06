using System.Text.Json;

using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Configuration;
using Orleans.TestingHost;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder sb)
    {
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
          },
          checkpointOptions =>
          {
              // Resolves the same container owned keyed client the rest of the stream provider uses, instead of
              // creating and disposing a private client for the checkpointer.
              checkpointOptions.ConnectionName = Constants.StreamProviderName;
              checkpointOptions.PersistInterval = TimeSpan.FromSeconds(10);
          })
          // Orleans persists streaming pub/sub subscription state through the store named "PubSubStore".
          // AddKurrentDBPubSubStore bundles the grain storage, Orleans serializer and explicit type mapping
          // that this Orleans internal state type requires.
          .AddKurrentDBPubSubStore(options =>
          {
              options.ConnectionName = Constants.PubSubStoreName;
              options.Credentials = new KurrentDBGrainStorageCredentialsOptions()
              {
                  UseDefault = true,
              };
          })
          .AddKurrentDBBasedLogConsistencyProviderAsDefault(options =>
          {
              options.ConnectionName = "journaledGrainLog";
              options.Credentials = new KurrentDBLogConsistentStorageCredentialsOptions()
              {
                  UseDefault = true,
              };
          });

        // A second provider reading $all. This is the shape the event sourcing storage is consumed through: the
        // events on those streams were not written by a stream provider, so they carry no Orleans metadata.
        sb.AddKurrentDBStreams(Constants.AllStreamProviderName, configurator =>
        {
            configurator.ConfigureKurrentDB(ob => ob.Configure(options =>
            {
                options.ConnectionName = Constants.AllStreamProviderName;
                options.Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true };
                options.Name = Constants.AllStreamProviderName;
                // A $all subscription is global, so this strategy allows exactly one queue.
                options.Queues = new List<string> { Constants.AllStreamQueueName };
            }));
            configurator.UseAllStreamSubscription(ob => ob.Configure(options =>
            {
                options.CheckpointConnectionName = Constants.AllStreamProviderName;
                // Without a filter the subscription would also see the events the other provider and the pub/sub
                // store write to the same container, which are unrelated to what these tests assert on. The prefix
                // is unique per process, so a run only ever reads events it appended itself.
                options.StreamFilterPrefix = Constants.EventSourcedStreamPrefix;
                // Reading the whole log is cheap because the filter scopes it to this run's own events, and it
                // removes any dependence on the subscription being established before the test appends its event.
                options.StartFromNow = false;
            }));
            configurator.UseKurrentDBCheckpointer(ob => ob.Configure(options =>
            {
                options.ConnectionName = Constants.AllStreamProviderName;
                options.PersistInterval = TimeSpan.FromSeconds(10);
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
            configurator.UseAllStreamSubscription(ob => ob.Configure(options =>
            {
                options.CheckpointConnectionName = Constants.ImplicitProviderName;
                options.StreamFilterPrefix = Constants.ImplicitStreamPrefix;
                options.StartFromNow = false;
            }));
            configurator.UseKurrentDBCheckpointer(ob => ob.Configure(options =>
            {
                options.ConnectionName = Constants.ImplicitProviderName;
                options.PersistInterval = TimeSpan.FromSeconds(10);
            }));
        });

        sb.ConfigureServices(services =>
        {
            var streamClientSettings = KurrentDBClientSettings.Create("kurrentdb://localhost:2113?tls=false");

            // The stream provider resolves its clients, serializer and type mapper by ConnectionName.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.StreamProviderName, (provider, ctx) =>
            {
                return provider.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
            });
            // There is no configuration connection string in the test host, so the explicit settings overload is used.
            services.AddKeyedKurrentDBPersistentSubscriptionsClient(Constants.StreamProviderName, streamClientSettings);
            services.AddSystemTextJsonStreamEventSerde(Constants.StreamProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.StreamProviderName, TypeMapKinds.DomainEvent);

            // The $all provider needs the same client, serializer and type mapper so it can convert events written
            // by the event sourcing storage into the Orleans cache format.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.AllStreamProviderName, (provider, ctx) =>
            {
                return provider.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
            });
            services.AddSystemTextJsonStreamEventSerde(Constants.AllStreamProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.AllStreamProviderName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            // The $all strategy owns its read position, so it needs a checkpoint store keyed by the same name.
            services.AddKeyedSingleton<ICheckpointStore<ulong>>(Constants.AllStreamProviderName, (provider, key) =>
            {
                return new KurrentDBCheckpointStore(provider.GetRequiredKeyedService<KurrentDBClient>(Constants.AllStreamProviderName));
            });

            // The implicit subscription provider needs the same set of keyed services.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.ImplicitProviderName, (provider, ctx) =>
            {
                return provider.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
            });
            services.AddSystemTextJsonStreamEventSerde(Constants.ImplicitProviderName);
            services.AddDefaultTypeMapAttributeMappers(Constants.ImplicitProviderName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddKeyedSingleton<ICheckpointStore<ulong>>(Constants.ImplicitProviderName, (provider, key) =>
            {
                return new KurrentDBCheckpointStore(provider.GetRequiredKeyedService<KurrentDBClient>(Constants.ImplicitProviderName));
            });

            services.AddKeyedSingleton<KurrentDBClient>("journaledGrainLog", (provider, ctx) =>
            {
                var clientSettings = KurrentDBClientSettings.Create("kurrentdb://localhost:2113?tls=false");
                return new KurrentDBClient(clientSettings);
            });

            // AddKurrentDBPubSubStore registers the storage serializer and type mapper for this store; the
            // keyed client is still supplied by the host.
            services.AddKeyedSingleton<KurrentDBClient>(Constants.PubSubStoreName, (provider, ctx) =>
            {
                return provider.GetRequiredKeyedService<KurrentDBClient>("journaledGrainLog");
            });

            services.AddSingleton<JsonSerializerOptions>(provider =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                return options;
            });
            services.AddDefaultTypeMapAttributeMappers("journaledGrainLog", TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddDefaultSystemTextJsonGrainStorageSerializer("journaledGrainLog");
        });
    }
}
