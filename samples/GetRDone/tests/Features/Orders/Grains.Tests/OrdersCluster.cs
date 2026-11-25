using Continuum.Hosting;
using Continuum.TypeMapping;

using GetRDone.Email;
using GetRDone.Orders.Grains.OrderAggregate;
using GetRDone.Orders.Grains.ReadModels;
using GetRDone.Orders.Grains.Tests.Data;

using ManagedCode.Orleans.Identity.Server.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;
using Orleans.TestingHost;

namespace GetRDone.Orders.Grains.Tests;

public sealed class OrdersSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorageAsDefault();
        siloBuilder.AddLogStorageBasedLogConsistencyProvider("EventLog");

        siloBuilder.AddOrleansIdentity();

        siloBuilder.Services.AddDefaultTypeMapAttributeMappers("kurrentdb", TypeMapKinds.DomainEvent);
        siloBuilder.Services.AddSingleton<IOrderPermissions, OrderPermissions>();
        siloBuilder.Services.AddSingleton<IReadModelSeedData, TestReadModelSeedData>();
        siloBuilder.Services.AddSingleton<RecordingEmailSender>();
        siloBuilder.Services.AddSingleton<IEmailSender>(services => services.GetRequiredService<RecordingEmailSender>());

        // Memory streams stand in for the KurrentDB stream providers, so the tests produce the events they would deliver.
        siloBuilder.AddMemoryStreams(OrderProjections.ProviderName, b => b.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly));
        siloBuilder.AddMemoryStreams(OrderReactors.ProviderName, b => b.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly));
        siloBuilder.AddMemoryStreams(NotificationReactors.ProviderName, b => b.ConfigureStreamPubSub(StreamPubSubType.ImplicitOnly));
        siloBuilder.AddMemoryGrainStorage(OrderProjections.StorageName);
        siloBuilder.Services.AddOptions<MemoryGrainStorageOptions>(OrderProjections.StorageName)
            .PostConfigure<Serializer>((options, serializer) => options.GrainStorageSerializer = new OrleansGrainStorageSerializer(serializer));
    }
}

public sealed class OrdersClientConfigurator : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        => clientBuilder
            .AddMemoryStreams(OrderProjections.ProviderName)
            .AddMemoryStreams(OrderReactors.ProviderName)
            .AddMemoryStreams(NotificationReactors.ProviderName);
}

public sealed class OrdersCluster : IAsyncLifetime
{
    public TestCluster Cluster { get; } = new TestClusterBuilder(1)
        .AddSiloBuilderConfigurator<OrdersSiloConfigurator>()
        .AddClientBuilderConfigurator<OrdersClientConfigurator>()
        .Build();

    /// <summary>The emails the silo sent, in place of Mailpit.</summary>
    public RecordingEmailSender EmailSender => Cluster.Primary is InProcessSiloHandle silo
        ? silo.SiloHost.Services.GetRequiredService<RecordingEmailSender>()
        : throw new InvalidOperationException("The test cluster's primary silo is not running in this process.");

    public async ValueTask InitializeAsync() => await Cluster.DeployAsync();
    public async ValueTask DisposeAsync() => await Cluster.StopAllSilosAsync();
}
