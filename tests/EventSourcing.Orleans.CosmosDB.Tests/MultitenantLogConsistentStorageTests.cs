using Continuum.EventSourcing.Orleans.CosmosDB.Tests.Events;
using Continuum.Hosting;
using Continuum.Hosting.Orleans;
using Continuum.Orleans.Tenancy;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public class MultitenantLogConsistentStorageTests
{
    private const string ProviderName = "multitenant";
    private const string SharedConnectionName = "shared";
    private const string DedicatedConnectionName = "dedicated";
    private const string DedicatedTenantId = "dedicated-tenant";
    private const string GrainTypeName = "snack";

    private static Testcontainers.CosmosDb.CosmosDbContainer Emulator => ClusterFixture.Emulator ?? throw new InvalidOperationException("The Cosmos DB emulator has not been started.");

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

    private static Task<(ServiceProvider Services, ILogConsistentStorage Storage)> StartStorageAsync(CosmosClient client, string dedicatedContainerName) =>
        StartStorageAsync(client, client, dedicatedContainerName, []);

    private static async Task<(ServiceProvider Services, ILogConsistentStorage Storage)> StartStorageAsync(CosmosClient sharedClient, CosmosClient dedicatedClient,
        string dedicatedContainerName, List<string> resolvedConnections)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.Configure<ClusterOptions>(options => options.ServiceId = "multitenant-test");
        services.AddKeyedSingleton(SharedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(SharedConnectionName);
            return sharedClient;
        });
        services.AddKeyedSingleton(DedicatedConnectionName, (_, _) =>
        {
            resolvedConnections.Add(DedicatedConnectionName);
            return dedicatedClient;
        });
        foreach (var connectionName in new[] { SharedConnectionName, DedicatedConnectionName })
        {
            services.AddDefaultTypeMapAttributeMappers(connectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
            services.AddTypeMappedJsonGrainStorageSerializer(connectionName);
        }
        services.AddMultitenantAzureCosmosDBBasedLogConsistencyProvider(ProviderName,
            (tenantId, options) =>
            {
                if (tenantId == DedicatedTenantId)
                {
                    options.ConnectionName = DedicatedConnectionName;
                    options.ContainerName = dedicatedContainerName;
                }
            },
            ob => ob.Configure(options =>
            {
                options.ConnectionName = SharedConnectionName;
                options.DatabaseName = ClusterFixture.DatabaseName;
                options.ContainerName = ClusterFixture.ContainerName;
            }));

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

    private static async Task<int> CountItemsAsync(Container container, GrainId grainId)
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE ENDSWITH(c.streamName, @grainId)")
            .WithParameter("@grainId", grainId.ToString());
        using var iterator = container.GetItemQueryIterator<int>(query);
        var count = 0;
        while (iterator.HasMoreResults)
        {
            foreach (var value in await iterator.ReadNextAsync(TestContext.Current.CancellationToken))
            {
                count += value;
            }
        }
        return count;
    }

    [Fact]
    public async Task Should_Route_Each_Tenant_To_Its_Configured_Container()
    {
        // The service provider owns and disposes the shared and dedicated clients, so containers are managed with
        // a separate client that outlives it.
        using var adminClient = ClusterFixture.CreateClient(Emulator);
        var client = ClusterFixture.CreateClient(Emulator);
        var dedicatedClient = ClusterFixture.CreateClient(Emulator);
        var database = adminClient.GetDatabase(ClusterFixture.DatabaseName);
        var dedicatedContainerName = $"tenant-{Guid.NewGuid():N}";
        var dedicatedContainer = (await database.CreateContainerAsync(dedicatedContainerName, "/streamName", cancellationToken: TestContext.Current.CancellationToken)).Container;
        var sharedContainer = database.GetContainer(ClusterFixture.ContainerName);
        try
        {
            var resolvedConnections = new List<string>();
            var (services, storage) = await StartStorageAsync(client, dedicatedClient, dedicatedContainerName, resolvedConnections);
            await using var _ = services;

            var plainGrain = GrainFor(null);
            var sharedTenantGrain = GrainFor("shared-tenant");
            var dedicatedTenantGrain = GrainFor(DedicatedTenantId);

            Assert.Equal(2, await storage.AppendAsync(GrainTypeName, plainGrain, CreateEvents(2), 0));
            Assert.Equal(2, await storage.AppendAsync(GrainTypeName, sharedTenantGrain, CreateEvents(2), 0));
            Assert.DoesNotContain(DedicatedConnectionName, resolvedConnections);

            Assert.Equal(2, await storage.AppendAsync(GrainTypeName, dedicatedTenantGrain, CreateEvents(2), 0));
            Assert.Contains(DedicatedConnectionName, resolvedConnections);

            Assert.True(await CountItemsAsync(sharedContainer, plainGrain) > 0);
            Assert.True(await CountItemsAsync(sharedContainer, sharedTenantGrain) > 0);
            Assert.Equal(0, await CountItemsAsync(sharedContainer, dedicatedTenantGrain));
            Assert.True(await CountItemsAsync(dedicatedContainer, dedicatedTenantGrain) > 0);
            Assert.Equal(0, await CountItemsAsync(dedicatedContainer, sharedTenantGrain));

            var read = await storage.ReadAsync<SnackEvent>(GrainTypeName, dedicatedTenantGrain, 0, 10);
            Assert.Equal(2, read.Count);
            Assert.Equal(2, await storage.GetLastVersionAsync(GrainTypeName, dedicatedTenantGrain));
        }
        finally
        {
            await dedicatedContainer.DeleteContainerAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Should_Fail_Tenant_Access_When_Its_Container_Does_Not_Exist_And_Retry_Later()
    {
        using var client = ClusterFixture.CreateClient(Emulator);
        var dedicatedContainerName = $"missing-{Guid.NewGuid():N}";
        var (services, storage) = await StartStorageAsync(client, dedicatedContainerName);
        await using var _ = services;
        var grainId = GrainFor(DedicatedTenantId);

        var ex = await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => storage.GetLastVersionAsync(GrainTypeName, grainId));
        Assert.Contains(dedicatedContainerName, ex.Message);

        var container = (await client.GetDatabase(ClusterFixture.DatabaseName).CreateContainerAsync(dedicatedContainerName, "/streamName", cancellationToken: TestContext.Current.CancellationToken)).Container;
        try
        {
            Assert.Equal(0, await storage.GetLastVersionAsync(GrainTypeName, grainId));
        }
        finally
        {
            await container.DeleteContainerAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Should_Reject_Invalid_Tenant_Options()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.Configure<ClusterOptions>(options => options.ServiceId = "multitenant-test");
        using var client = ClusterFixture.CreateClient(Emulator);
        services.AddKeyedSingleton(SharedConnectionName, client);
        services.AddDefaultTypeMapAttributeMappers(SharedConnectionName, TypeMapKinds.DomainEvent | TypeMapKinds.Metadata);
        services.AddTypeMappedJsonGrainStorageSerializer(SharedConnectionName);
        services.AddMultitenantAzureCosmosDBBasedLogConsistencyProvider(ProviderName,
            (tenantId, options) => options.ContainerName = null!,
            ob => ob.Configure(options =>
            {
                options.ConnectionName = SharedConnectionName;
                options.DatabaseName = ClusterFixture.DatabaseName;
                options.ContainerName = ClusterFixture.ContainerName;
            }));
        await using var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredKeyedService<ILogConsistentStorage>(ProviderName);

        await Assert.ThrowsAsync<OrleansConfigurationException>(() => storage.GetLastVersionAsync(GrainTypeName, GrainFor("acme")));
    }
}
