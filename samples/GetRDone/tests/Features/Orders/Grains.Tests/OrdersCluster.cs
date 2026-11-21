using Continuum.Hosting;
using Continuum.TypeMapping;

using Orleans.TestingHost;

namespace GetRDone.Orders.Tests;

public sealed class OrdersSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.AddLogStorageBasedLogConsistencyProvider("EventLog");
        siloBuilder.Services.AddDefaultTypeMapAttributeMappers("kurrentdb", TypeMapKinds.DomainEvent);
    }
}

public sealed class OrdersCluster : IAsyncLifetime
{
    public TestCluster Cluster { get; } = new TestClusterBuilder(1)
        .AddSiloBuilderConfigurator<OrdersSiloConfigurator>()
        .Build();

    public async ValueTask InitializeAsync() => await Cluster.DeployAsync();
    public async ValueTask DisposeAsync() => await Cluster.StopAllSilosAsync();
}
