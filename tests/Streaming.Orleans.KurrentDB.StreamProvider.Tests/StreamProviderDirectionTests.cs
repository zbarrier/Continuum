using Continuum.Orleans.Events;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that catch-up stream providers are read only, so nothing can write into streams owned by event sourced
///     grains, while persistent subscription providers can still be written to.
/// </summary>
public class StreamProviderDirectionTests
{
    private const string ProviderName = "test-provider";

    private static ServiceProvider BuildServices(Action<ISiloKurrentDBStreamConfigurator> configure)
    {
        var services = new ServiceCollection();
        services.AddOptions<LoadSheddingOptions>();
        services.AddOptions<ClusterOptions>().Configure(options => options.ServiceId = "test-service");
        services.AddSingleton<ICheckpointStore<ulong>, FakeCheckpointStore>();
        services.AddOptions<KurrentDBDataAdapterOptions>(ProviderName).Configure(options =>
        {
            var typeMapper = new MarkerTypeMapper();
            typeMapper.AddType(typeof(CheckpointMarker), CheckpointMarker.TypeName);
            options.TypeMapper = typeMapper;
            options.GrainStorageSerializer = new EmptySerializer();
        });
        var configurator = new SiloKurrentDBStreamConfigurator(ProviderName, configureServices => configureServices(services));
        configure(configurator);
        return services.BuildServiceProvider();
    }

    private static KurrentDBQueueAdapter CreateAdapter(IServiceProvider services, IKurrentDBReceiverFactory strategy) =>
        new(ProviderName,
            new KurrentDBOptions { Name = ProviderName, ConnectionName = ProviderName, Queues = ["queue-0"] },
            strategy,
            new HashRingBasedPartitionedStreamQueueMapper(["queue-0"], ProviderName),
            (_, _) => throw new NotSupportedException(),
            (_, _) => throw new NotSupportedException(),
            (_, _) => throw new NotSupportedException(),
            new ThrowingDataAdapter(),
            services,
            NullLoggerFactory.Instance,
            null);

    [Fact]
    public void The_Catch_Up_Strategy_Is_Read_Only()
    {
        using var services = BuildServices(configurator => configurator.UseCatchUpSubscription());

        var strategy = services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName);

        Assert.IsType<KurrentDBCatchUpSubscriptionReceiverFactory>(strategy);
        Assert.Equal(StreamProviderDirection.ReadOnly, strategy.Direction);
    }

    [Fact]
    public void The_Catch_Up_Strategy_Refuses_To_Create_A_Producer()
    {
        using var services = BuildServices(configurator => configurator.UseCatchUpSubscription());

        var strategy = services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName);

        Assert.Throws<NotSupportedException>(() => strategy.CreateProducer("queue-0", NullLoggerFactory.Instance));
    }

    [Fact]
    public void The_Persistent_Subscription_Strategy_Is_Read_Write()
    {
        using var services = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        var strategy = services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName);

        Assert.IsType<KurrentDBPersistentSubscriptionReceiverFactory>(strategy);
        Assert.Equal(StreamProviderDirection.ReadWrite, strategy.Direction);
    }

    [Fact]
    public void The_Adapter_Takes_Its_Direction_From_The_Strategy()
    {
        using var catchUpServices = BuildServices(configurator => configurator.UseCatchUpSubscription());
        using var persistentServices = BuildServices(configurator => configurator.UsePersistentSubscriptions());

        var catchUp = CreateAdapter(catchUpServices, catchUpServices.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName));
        var persistent = CreateAdapter(persistentServices, persistentServices.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName));

        Assert.Equal(StreamProviderDirection.ReadOnly, catchUp.Direction);
        Assert.Equal(StreamProviderDirection.ReadWrite, persistent.Direction);
    }

    [Fact]
    public async Task A_Catch_Up_Adapter_Refuses_To_Queue_Events_Before_Converting_Them()
    {
        using var services = BuildServices(configurator => configurator.UseCatchUpSubscription());
        var adapter = CreateAdapter(services, services.GetRequiredKeyedService<IKurrentDBReceiverFactory>(ProviderName));

        // The data adapter throws if called, so this also proves nothing is converted or appended.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            adapter.QueueMessageBatchAsync(StreamId.Create("ns", "key"), new[] { "event" }, null, null));

        Assert.Contains(ProviderName, exception.Message);
    }

    private sealed class MarkerTypeMapper : TypeMapper;

    private sealed class EmptySerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => BinaryData.FromBytes(Array.Empty<byte>());

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class FakeCheckpointStore : ICheckpointStore<ulong>
    {
        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) => Task.FromResult(0UL);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ThrowingDataAdapter : IKurrentDBDataAdapter
    {
        public global::KurrentDB.Client.EventData[] ToQueueMessage<T>(StreamId streamId, IEnumerable<T> events, StreamSequenceToken? token, Dictionary<string, object>? requestContext) =>
            throw new InvalidOperationException("The data adapter must not be called.");

        public IBatchContainer GetBatchContainer(ref CachedMessage cachedMessage) => throw new NotSupportedException();

        public StreamSequenceToken GetSequenceToken(ref CachedMessage cachedMessage) => throw new NotSupportedException();

        public CachedMessage FromQueueMessage(StreamPosition streamPosition, StreamedEvent<KurrentDBRawEvent> queueMessage, DateTime dequeueTime, Func<int, ArraySegment<byte>> getSegment) =>
            throw new NotSupportedException();

        public StreamPosition GetStreamPosition(StreamedEvent<KurrentDBRawEvent> queueMessage) => throw new NotSupportedException();

        public ulong GetCommitPosition(CachedMessage cachedMessage) => throw new NotSupportedException();

        public StreamId GetStreamId(StreamedEvent<KurrentDBRawEvent> queueMessage) => throw new NotSupportedException();
    }
}
