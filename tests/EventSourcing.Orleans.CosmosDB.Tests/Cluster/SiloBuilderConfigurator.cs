using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

public class TestSiloConfigurations : ISiloConfigurator
{
    public const string ConnectionName = "journaledGrainLog";

    public void Configure(ISiloBuilder sb)
    {
        sb.AddMemoryGrainStorage("MemoryStorageProvider")
          .AddAzureCosmosDBBasedLogConsistencyProviderAsDefault(options =>
          {
              options.ConnectionName = ConnectionName;
              options.DatabaseName = ClusterFixture.DatabaseName;
              options.ContainerName = ClusterFixture.ContainerName;
          });

        sb.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<CosmosClient>(ConnectionName, (provider, key) =>
            {
                var emulator = ClusterFixture.Emulator
                    ?? throw new InvalidOperationException("The Cosmos DB emulator has not been started.");
                return ClusterFixture.CreateClient(emulator);
            });

            services.AddDefaultTypeMapAttributeMappers(ConnectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddTypeMappedJsonGrainStorageSerializer(ConnectionName);
        });
    }
}
