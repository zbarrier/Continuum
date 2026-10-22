using System.Net;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using KurrentDB.Client;

using Orleans.TestingHost;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

public sealed class ClusterFixture : IAsyncLifetime
{
    public const string ConnectionStringKey = "KurrentDBConnectionString";

    private const int KurrentDBPort = 2113;

    // The container is removed on dispose so every run starts against an empty log. Reusing a container left the log
    // in place between runs, and because the $all provider reads from the beginning, each run replayed everything
    // earlier runs had written until delivery outgrew the tests' timeouts.
    // Pinned so a new KurrentDB release cannot change test behavior unnoticed.
    // A random host port lets parallel runs and leftover containers coexist.
    private readonly IContainer _container = new ContainerBuilder("docker.kurrent.io/kurrent-latest/kurrentdb:25.1")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .WithPortBinding(KurrentDBPort, true)
        .WithEnvironment("KURRENTDB_INSECURE", "true")
        .WithEnvironment("KURRENTDB_MEM_DB", "true")
        .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
        .WithEnvironment("KURRENTDB_RUN_PROJECTIONS", "All")
        .WithEnvironment("KURRENTDB_START_STANDARD_PROJECTIONS", "true")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(r => r
                .ForPort(KurrentDBPort)
                .ForPath("/health/live")
                .ForStatusCode(HttpStatusCode.NoContent)
                .ForStatusCode(HttpStatusCode.OK)))
        .Build();

    private TestCluster? _cluster;
    private string? _connectionString;

    public TestCluster Cluster => _cluster ?? throw new InvalidOperationException("The cluster has not been deployed.");

    public string ConnectionString => _connectionString ?? throw new InvalidOperationException("The KurrentDB container has not been started.");

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        _connectionString = FormattableString.Invariant(
            $"kurrentdb://{_container.Hostname}:{_container.GetMappedPublicPort(KurrentDBPort)}?tls=false");
        await WaitUntilReadyAsync(_connectionString);

        var builder = new TestClusterBuilder();
        builder.Properties[ConnectionStringKey] = _connectionString;
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        _cluster = builder.Build();
        await _cluster.DeployAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_cluster is not null)
        {
            await _cluster.StopAllSilosAsync();
            await _cluster.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    // /health/live passes once the process is up, before the node has become leader and accepts writes, so retry a
    // write for a while. Otherwise the first test can fail with "Server Is Not Ready".
    private static async Task WaitUntilReadyAsync(string connectionString)
    {
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(connectionString));
        var deadline = DateTime.UtcNow.AddMinutes(1);
        while (true)
        {
            try
            {
                await client.AppendToStreamAsync($"readiness-{Guid.NewGuid():N}", StreamState.NoStream,
                    [new EventData(Uuid.NewUuid(), "readiness", ReadOnlyMemory<byte>.Empty)]);
                return;
            }
            catch (Exception) when (DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }
    }
}
