using Continuum.EventSourcing.Orleans.KurrentDB.Tests.Events;
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

namespace Continuum.EventSourcing.Orleans.KurrentDB.Tests;

[Collection(ClusterCollection.Name)]
public class MultitenantLogConsistentStorageTests(ClusterFixture fixture)
{
    private const string ProviderName = "multitenant";
    private const string SharedConnectionName = "shared";
    private const string DedicatedConnectionName = "dedicated";
    private const string DedicatedTenantId = "dedicated-tenant";
    private const string GrainTypeName = "snack";

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

    private static SnackEvent[] CreateEvents(int count)
    {
        var id = Guid.NewGuid();
        var events = new SnackEvent[count];
        events[0] = new SnackInitializedEvent(id, "Name0", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", 1);
        for (var index = 1; index < count; index++)
        {
            events[index] = new SnackNameChangedEvent(id, $"Name{index}", Guid.NewGuid(), DateTimeOffset.UtcNow, "Test", index + 1);
        }
        return events;
    }

    private async Task<(ServiceProvider Services, ILogConsistentStorage Storage)> StartStorageAsync(
        Func<KurrentDBClient> createDedicatedClient, List<string> resolvedConnections,
        Action<string, KurrentDBLogConsistentStorageOptions>? configureTenantOptions = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.Configure<ClusterOptions>(options => options.ServiceId = "multitenant-test");
        services.AddKeyedSingleton(SharedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(SharedConnectionName);
            return new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString));
        });
        services.AddKeyedSingleton(DedicatedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(DedicatedConnectionName);
            return createDedicatedClient();
        });
        foreach (var connectionName in new[] { SharedConnectionName, DedicatedConnectionName })
        {
            services.AddDefaultTypeMapAttributeMappers(connectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddTypeMappedJsonGrainStorageSerializer(connectionName);
        }
        services.AddMultitenantKurrentDBBasedLogConsistencyProvider(ProviderName,
            configureTenantOptions ?? ((tenantId, options) =>
            {
                if (tenantId == DedicatedTenantId)
                {
                    options.ConnectionName = DedicatedConnectionName;
                }
            }),
            ob => ob.Configure(options => options.ConnectionName = SharedConnectionName));

        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredKeyedService<ILogConsistentStorage>(ProviderName);
        var lifecycle = new CapturingLifecycle();
        ((ILifecycleParticipant<ISiloLifecycle>)storage).Participate(lifecycle);
        foreach (var start in lifecycle.OnStart)
        {
            await start(TestContext.Current.CancellationToken);
        }
        return (provider, storage);
    }

    private async Task<long> CountEventsAsync(string streamName)
    {
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString));
        var result = client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, cancellationToken: TestContext.Current.CancellationToken);
        if (await result.ReadState == ReadState.StreamNotFound)
        {
            return 0;
        }
        return await result.CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Route_Each_Tenant_To_Its_Configured_Connection()
    {
        var resolvedConnections = new List<string>();
        var (services, storage) = await StartStorageAsync(
            () => new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString)), resolvedConnections);
        await using var _ = services;

        var plainGrain = GrainFor(null);
        var sharedTenantGrain = GrainFor("shared-tenant");
        var dedicatedTenantGrain = GrainFor(DedicatedTenantId);

        Assert.Equal(2, await storage.AppendAsync(GrainTypeName, plainGrain, CreateEvents(2), 0));
        Assert.Equal(2, await storage.AppendAsync(GrainTypeName, sharedTenantGrain, CreateEvents(2), 0));
        Assert.DoesNotContain(DedicatedConnectionName, resolvedConnections);

        Assert.Equal(2, await storage.AppendAsync(GrainTypeName, dedicatedTenantGrain, CreateEvents(2), 0));
        Assert.Contains(DedicatedConnectionName, resolvedConnections);

        Assert.Equal(2, await CountEventsAsync($"{GrainTypeName}-{plainGrain.Key}"));
        Assert.Equal(2, await CountEventsAsync($"{GrainTypeName}-{sharedTenantGrain.Key}"));
        Assert.Equal(2, await CountEventsAsync($"{GrainTypeName}-{dedicatedTenantGrain.Key}"));
        Assert.StartsWith($"{GrainTypeName}-shared-tenant|", $"{GrainTypeName}-{sharedTenantGrain.Key}");

        var read = await storage.ReadAsync<SnackEvent>(GrainTypeName, dedicatedTenantGrain, 0, 10);
        Assert.Equal(2, read.Count);
        Assert.Equal(2, await storage.GetLastVersionAsync(GrainTypeName, dedicatedTenantGrain));
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
                : new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString));
        }, []);
        await using var _ = services;
        var grainId = GrainFor(DedicatedTenantId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => storage.GetLastVersionAsync(GrainTypeName, grainId));

        Assert.Equal(0, await storage.GetLastVersionAsync(GrainTypeName, grainId));
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task Should_Reject_Invalid_Tenant_Options()
    {
        var (services, storage) = await StartStorageAsync(
            () => new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString)), [],
            (tenantId, options) => options.ConnectionName = null!);
        await using var _ = services;

        await Assert.ThrowsAsync<OrleansConfigurationException>(() => storage.GetLastVersionAsync(GrainTypeName, GrainFor("acme")));
    }
}
