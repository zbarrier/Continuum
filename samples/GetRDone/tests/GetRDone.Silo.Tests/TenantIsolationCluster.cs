using Orleans.TestingHost;

namespace GetRDone.Silo.Tests;

public sealed class TenantIsolationSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder) => siloBuilder.AddTenantIsolation();
}

/// <summary>A test silo with only the silo's tenant isolation, to call the test grains with.</summary>
public sealed class TenantIsolationCluster : IAsyncLifetime
{
    public TestCluster Cluster { get; } = new TestClusterBuilder(1)
        .AddSiloBuilderConfigurator<TenantIsolationSiloConfigurator>()
        .Build();

    public async ValueTask InitializeAsync() => await Cluster.DeployAsync();
    public async ValueTask DisposeAsync() => await Cluster.StopAllSilosAsync();
}
