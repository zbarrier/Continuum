using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Storage;

namespace Continuum.Persistence.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class MultitenantKurrentDBGrainStorageRoutingTests(ClusterFixture fixture)
{
    private const string ProviderName = "multitenant-routing";
    private const string SharedConnectionName = "shared";
    private const string DedicatedConnectionName = "dedicated";
    private const string DedicatedTenantId = "dedicated-tenant";
    private const string GrainTypeName = "counter";

    private sealed class CapturingLifecycle : ISiloLifecycle
    {
        public List<Func<CancellationToken, Task>> OnStart { get; } = [];

        public int HighestCompletedStage => 0;

        public int LowestStoppedStage => 0;

        public IDisposable Subscribe(string observerName, int stage, ILifecycleObserver observer)
        {
            OnStart.Add(observer.OnStart);
            return new NoopDisposable();
        }

        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    private static GrainId GrainFor(string? tenantId) =>
        GrainId.Create(GrainTypeName, TenantKey.Create(tenantId, Guid.NewGuid().ToString("N")));

    private static GrainState<CounterState> NewState(int count = 0) => new(new CounterState { Count = count });

    private KurrentDBClient CreateClient() => new(KurrentDBClientSettings.Create(fixture.ConnectionString));

    private async Task<(ServiceProvider Services, IGrainStorage Storage)> StartStorageAsync(
        Func<KurrentDBClient> createDedicatedClient, List<string> resolvedConnections,
        Action<string, KurrentDBGrainStorageOptions>? configureTenantOptions = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.Configure<ClusterOptions>(options => options.ServiceId = "multitenant-test");
        services.AddKeyedSingleton(SharedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(SharedConnectionName);
            return CreateClient();
        });
        services.AddKeyedSingleton(DedicatedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(DedicatedConnectionName);
            return createDedicatedClient();
        });
        foreach (var connectionName in new[] { SharedConnectionName, DedicatedConnectionName })
        {
            services.AddDefaultTypeMapAttributeMappers(connectionName, TypeMapKinds.Snapshot);
            services.AddTypeMappedJsonGrainStorageSerializer(connectionName);
        }
        services.AddMultitenantKurrentDBGrainStorage(ProviderName,
            configureTenantOptions ?? ((tenantId, options) =>
            {
                if (tenantId == DedicatedTenantId)
                {
                    options.ConnectionName = DedicatedConnectionName;
                }
            }),
            ob => ob.Configure(options => options.ConnectionName = SharedConnectionName));

        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredKeyedService<IGrainStorage>(ProviderName);
        var lifecycle = new CapturingLifecycle();
        ((ILifecycleParticipant<ISiloLifecycle>)storage).Participate(lifecycle);
        foreach (var start in lifecycle.OnStart)
        {
            await start(TestContext.Current.CancellationToken);
        }
        return (provider, storage);
    }

    [Fact]
    public async Task Should_Resolve_Dedicated_Connection_Only_For_Dedicated_Tenant()
    {
        var resolvedConnections = new List<string>();
        var (services, storage) = await StartStorageAsync(CreateClient, resolvedConnections);
        await using var _ = services;

        await storage.WriteStateAsync(GrainTypeName, GrainFor(null), NewState(1));
        await storage.WriteStateAsync(GrainTypeName, GrainFor("shared-tenant"), NewState(2));
        Assert.DoesNotContain(DedicatedConnectionName, resolvedConnections);

        var dedicatedGrain = GrainFor(DedicatedTenantId);
        await storage.WriteStateAsync(GrainTypeName, dedicatedGrain, NewState(3));
        Assert.Contains(DedicatedConnectionName, resolvedConnections);

        var read = NewState();
        await storage.ReadStateAsync(GrainTypeName, dedicatedGrain, read);
        Assert.True(read.RecordExists);
        Assert.Equal(3, read.State!.Count);
    }

    [Fact]
    public async Task Should_Retry_Tenant_Storage_Creation_After_A_Failure()
    {
        var attempts = 0;
        var (services, storage) = await StartStorageAsync(() =>
        {
            attempts++;
            return attempts == 1
                ? throw new InvalidOperationException("Dedicated connection is not available yet.")
                : CreateClient();
        }, []);
        await using var _ = services;
        var grainId = GrainFor(DedicatedTenantId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.ReadStateAsync(GrainTypeName, grainId, NewState()));

        var state = NewState();
        await storage.ReadStateAsync(GrainTypeName, grainId, state);
        Assert.False(state.RecordExists);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task Should_Reject_Invalid_Tenant_Options()
    {
        var (services, storage) = await StartStorageAsync(CreateClient, [],
            (tenantId, options) => options.ConnectionName = null!);
        await using var _ = services;

        await Assert.ThrowsAsync<OrleansConfigurationException>(() => storage.ReadStateAsync(GrainTypeName, GrainFor("acme"), NewState()));
    }
}
