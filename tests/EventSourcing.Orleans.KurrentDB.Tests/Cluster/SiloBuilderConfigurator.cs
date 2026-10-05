using System.Text.Json;

using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Orleans.Configuration;
using Orleans.TestingHost;

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder sb)
    {
        sb.AddMemoryGrainStorage("MemoryStorageProvider")
          .AddKurrentDBBasedLogConsistencyProviderAsDefault(options =>
          {
              options.ConnectionName = "journaledGrainLog";
              options.Credentials = new KurrentDBLogConsistentStorageCredentialsOptions()
              {
                  UseDefault = true,
              };
          });

        sb.ConfigureServices(services => 
        {
            services.AddKeyedSingleton<KurrentDBClient>("journaledGrainLog", (provider, ctx) =>
            {
                var connectionString = sb.Configuration[ClusterFixture.ConnectionStringKey]
                    ?? throw new InvalidOperationException($"{ClusterFixture.ConnectionStringKey} was not provided to the silo.");
                var clientSettings = KurrentDBClientSettings.Create(connectionString);
                return new KurrentDBClient(clientSettings);
            });

            services.AddDefaultTypeMapAttributeMappers("journaledGrainLog", TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddTypeMappedJsonGrainStorageSerializer("journaledGrainLog");
        });
    }
}
