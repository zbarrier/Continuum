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
    public const string MultitenantProvider = "kurrent-multitenant";
    public const string DedicatedConnectionName = "dedicatedGrainState";
    public const string DedicatedTenantId = "dedicated-tenant";

    public void Configure(ISiloBuilder sb)
    {
        sb.AddMultitenantKurrentDBGrainStorage(MultitenantProvider,
            (tenantId, options) =>
            {
                if (tenantId == DedicatedTenantId)
                {
                    options.ConnectionName = DedicatedConnectionName;
                }
            },
            ob => ob.Configure(options => options.ConnectionName = ConnectionName));

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

            services.AddKeyedSingleton<KurrentDBClient>(DedicatedConnectionName, (provider, ctx) =>
            {
                var connectionString = sb.Configuration[ClusterFixture.ConnectionStringKey]
                    ?? throw new InvalidOperationException($"{ClusterFixture.ConnectionStringKey} was not provided to the silo.");
                return new KurrentDBClient(KurrentDBClientSettings.Create(connectionString));
            });

            services.AddDefaultTypeMapAttributeMappers(ConnectionName, TypeMapKinds.Snapshot);
            services.AddTypeMappedJsonGrainStorageSerializer(ConnectionName);
            services.AddDefaultTypeMapAttributeMappers(DedicatedConnectionName, TypeMapKinds.Snapshot);
            services.AddTypeMappedJsonGrainStorageSerializer(DedicatedConnectionName);
        });
    }
}
