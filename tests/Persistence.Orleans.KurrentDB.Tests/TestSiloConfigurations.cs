using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;

using Orleans.TestingHost;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

public class TestSiloConfigurations : ISiloConfigurator
{
    public const string ConnectionName = "grainState";
    public const string MarkerProvider = "kurrent-marker";
    public const string DeleteProvider = "kurrent-delete";
    public const string UnlimitedProvider = "kurrent-unlimited";

    public void Configure(ISiloBuilder sb)
    {
        sb.AddKurrentDBGrainStorage(MarkerProvider, options => options.ConnectionName = ConnectionName)
          .AddKurrentDBGrainStorage(UnlimitedProvider, options =>
          {
              options.ConnectionName = ConnectionName;
              options.MaxStateEventCount = null;
          })
          .AddKurrentDBGrainStorage(DeleteProvider, options =>
          {
              options.ConnectionName = ConnectionName;
              options.DeleteStateOnClear = true;
          });

        sb.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<KurrentDBClient>(ConnectionName, (provider, ctx) =>
            {
                var connectionString = sb.Configuration[ClusterFixture.ConnectionStringKey]
                    ?? throw new InvalidOperationException($"{ClusterFixture.ConnectionStringKey} was not provided to the silo.");
                return new KurrentDBClient(KurrentDBClientSettings.Create(connectionString));
            });

            services.AddDefaultTypeMapAttributeMappers(ConnectionName, TypeMapKinds.Snapshot);
            services.AddTypeMappedJsonGrainStorageSerializer(ConnectionName);
        });
    }
}
