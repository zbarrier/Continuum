using System.Net;

using DotNet.Testcontainers.Builders;

using Orleans.TestingHost;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

public class ClusterFixture : IDisposable
{
    DotNet.Testcontainers.Containers.IContainer? _container;

    public ClusterFixture()
    {
        // The container is named per run and removed on dispose so every run starts against an empty log. A fixed
        // name left the log in place between runs, and because the $all provider reads from the beginning, each run
        // replayed everything earlier runs had written until delivery outgrew the tests' timeouts.
        _container = new ContainerBuilder("docker.kurrent.io/kurrent-latest/kurrentdb:latest")
            .WithName($"kurrentdb-orleans-eventsourcing-test-{Guid.NewGuid():N}")
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .WithPortBinding(2113, 2113)
            .WithEnvironment("KURRENTDB_INSECURE", "true")
            .WithEnvironment("KURRENTDB_ENABLE_ATOM_PUB_OVER_HTTP", "true")
            .WithEnvironment("KURRENTDB_RUN_PROJECTIONS", "All")
            .WithEnvironment("KURRENTDB_START_STANDARD_PROJECTIONS", "true")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r
                    .ForPort(2113)
                    .ForPath("/health/live")
                    .ForStatusCode(HttpStatusCode.NoContent)
                    .ForStatusCode(HttpStatusCode.OK)))
            .Build();

        _container.StartAsync().GetAwaiter().GetResult();

        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();

        _container?.DisposeAsync().GetAwaiter().GetResult();
    }

    public TestCluster Cluster { get; private set; }
}
