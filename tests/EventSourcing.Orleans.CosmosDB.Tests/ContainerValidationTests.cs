using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Continuum.EventSourcing.Orleans.CosmosDB.Tests;

[Collection(ClusterCollection.Name)]
public class ContainerValidationTests
{
    private const string ConnectionName = "validation";

    // The collection fixture guarantees the emulator and the shared database exist.
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

    private async Task StartStorageAsync(CosmosClient client, string containerName, TimeSpan? startupConnectionTimeout = null)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton(ConnectionName, client);
        var options = new CosmosDBLogConsistentStorageOptions
        {
            ConnectionName = ConnectionName,
            DatabaseName = ClusterFixture.DatabaseName,
            ContainerName = containerName,
        };
        if (startupConnectionTimeout is { } timeout)
        {
            options.StartupConnectionTimeout = timeout;
        }
        var storage = new CosmosDBLogConsistentStorage(services.BuildServiceProvider(), "validation", options,
            Options.Create(new ClusterOptions { ServiceId = "test" }), NullLogger<CosmosDBLogConsistentStorage>.Instance);

        var lifecycle = new CapturingLifecycle();
        storage.Participate(lifecycle);
        foreach (var start in lifecycle.OnStart)
        {
            await start(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task Should_Fail_To_Start_When_Cosmos_Is_Unreachable_After_Retrying()
    {
        // Nothing listens on this port, so every attempt fails with a connection error.
        using var client = new CosmosClient("https://127.0.0.1:1/", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
            new CosmosClientOptions { ConnectionMode = ConnectionMode.Gateway, LimitToEndpoint = true });

        var ex = await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(
            () => StartStorageAsync(client, ClusterFixture.ContainerName, TimeSpan.FromSeconds(3)));

        Assert.Contains("Could not connect", ex.Message);
    }

    [Fact]
    public async Task Should_Start_When_Container_Is_Correctly_Configured()
    {
        using var client = ClusterFixture.CreateClient(Emulator);

        await StartStorageAsync(client, ClusterFixture.ContainerName);
    }

    [Fact]
    public async Task Should_Fail_To_Start_When_Container_Does_Not_Exist()
    {
        using var client = ClusterFixture.CreateClient(Emulator);
        var containerName = $"missing-{Guid.NewGuid():N}";

        var ex = await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => StartStorageAsync(client, containerName));

        Assert.Contains(containerName, ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Should_Fail_To_Start_When_Partition_Key_Is_Wrong()
    {
        using var client = ClusterFixture.CreateClient(Emulator);
        var containerName = $"wrongpk-{Guid.NewGuid():N}";
        var container = (await client.GetDatabase(ClusterFixture.DatabaseName).CreateContainerAsync(containerName, "/id", cancellationToken: TestContext.Current.CancellationToken)).Container;
        try
        {
            var ex = await Assert.ThrowsAsync<CosmosDBLogConsistentStorageException>(() => StartStorageAsync(client, containerName));

            Assert.Contains("/id", ex.Message);
            Assert.Contains("/streamName", ex.Message);
        }
        finally
        {
            await container.DeleteContainerAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
    }
}
