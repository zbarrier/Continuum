using System.Diagnostics.Metrics;

using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.StatisticMonitors;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that the cache factory hands the selected strategy everything it needs to build a queue cache, shares one
///     block pool across queues, and that the catch-up strategy builds its cursor tracking cache from that context.
/// </summary>
public class QueueCacheFactoryWiringTests
{
    private const string ProviderName = "test-provider";

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly OrleansInstruments Instruments = new(new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>());

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => BinaryData.FromBytes(Array.Empty<byte>());

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static readonly KurrentDBQueueDataAdapter Adapter = new(OrleansSerializer, new KurrentDBDataAdapterOptions
    {
        ConnectionName = "test",
        ProviderName = ProviderName,
        TypeMapper = new FakeTypeMapper(),
        GrainStorageSerializer = new FakeSerializer(),
    });

    private sealed class RecordingStrategy : IKurrentDBReceiverFactory
    {
        public List<KurrentDBQueueCacheContext> Contexts { get; } = [];

        public List<RecordingCache> Caches { get; } = [];

        public KurrentDBReceiverOptionsBase ReceiverOptions => throw new NotSupportedException();

        public StreamProviderDirection Direction => StreamProviderDirection.ReadOnly;

        public IKurrentDBQueueCache CreateQueueCache(KurrentDBQueueCacheContext context)
        {
            Contexts.Add(context);
            var cache = new RecordingCache();
            Caches.Add(cache);
            return cache;
        }

        public IKurrentDBReceiver Create(KurrentDBReceiverSettings settings, ILogger logger) => throw new NotSupportedException();

        public IKurrentDBQueueAdapterReceiver CreateQueueAdapterReceiver(KurrentDBQueueAdapterReceiverContext context) => throw new NotSupportedException();

        public IKurrentDBProducer CreateProducer(string queueName, ILoggerFactory loggerFactory) => throw new NotSupportedException();
    }

    private sealed class RecordingCache : IKurrentDBQueueCache
    {
        public List<ICachePressureMonitor> PressureMonitors { get; } = [];

        public void AddCachePressureMonitor(ICachePressureMonitor monitor) => PressureMonitors.Add(monitor);

        public List<global::Orleans.Streams.StreamPosition> Add(IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> queueMessages, DateTime dequeueTimeUtc) => throw new NotSupportedException();

        public object GetCursor(StreamId streamId, StreamSequenceToken? sequenceToken) => throw new NotSupportedException();

        public object GetCursorAtPosition(StreamId streamId, StreamSubscriptionStartPosition startPosition) => throw new NotSupportedException();

        public bool TryGetNextMessage(object cursorObj, out IBatchContainer container) => throw new NotSupportedException();

        public void SignalPurge() => throw new NotSupportedException();

        public int GetMaxAddCount() => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }

    private static KurrentDBQueueCacheFactory CreateFactory(IKurrentDBReceiverFactory strategy, KurrentDBStreamCachePressureOptions? pressureOptions = null, StreamCacheEvictionOptions? evictionOptions = null) =>
        new(strategy,
            pressureOptions ?? new KurrentDBStreamCachePressureOptions(),
            new KurrentDBStreamCacheBufferPoolOptions(),
            evictionOptions ?? new StreamCacheEvictionOptions(),
            new StreamStatisticOptions(),
            Adapter,
            new KurrentDBMonitorAggregationDimensions(ProviderName),
            Instruments,
            TimeProvider.System);

    [Fact]
    public void The_Strategy_Receives_The_Queue_And_Shared_Settings()
    {
        var strategy = new RecordingStrategy();
        var evictionOptions = new StreamCacheEvictionOptions { MetadataMinTimeInCache = TimeSpan.FromMinutes(7) };
        var factory = CreateFactory(strategy, evictionOptions: evictionOptions);

        var cache = factory.CreateCache("queue-0", NullLoggerFactory.Instance);

        var context = Assert.Single(strategy.Contexts);
        Assert.Same(strategy.Caches[0], cache);
        Assert.Equal("queue-0", context.Queue);
        Assert.Same(Adapter, context.DataAdapter);
        Assert.IsType<KurrentDBQueueCacheBufferPool>(context.BufferPool);
        Assert.NotNull(context.TimePurge);
        Assert.NotNull(context.CacheMonitor);
        Assert.Same(NullLoggerFactory.Instance, context.LoggerFactory);
        Assert.Equal($"{ProviderName}.queue-0", context.LoggerNameSuffix);
        Assert.Equal(TimeSpan.FromMinutes(7), context.MetadataMinTimeInCache);
    }

    [Fact]
    public void Every_Queue_Shares_One_Block_Pool()
    {
        var strategy = new RecordingStrategy();
        var factory = CreateFactory(strategy);

        factory.CreateCache("queue-0", NullLoggerFactory.Instance);
        factory.CreateCache("queue-1", NullLoggerFactory.Instance);

        Assert.Equal(2, strategy.Contexts.Count);
        Assert.Same(strategy.Contexts[0].BufferPool, strategy.Contexts[1].BufferPool);
        Assert.NotSame(strategy.Contexts[0].CacheMonitor, strategy.Contexts[1].CacheMonitor);
    }

    [Fact]
    public void No_Pressure_Monitors_Are_Added_When_None_Are_Configured()
    {
        var strategy = new RecordingStrategy();
        var factory = CreateFactory(strategy, new KurrentDBStreamCachePressureOptions
        {
            AveragingCachePressureMonitorFlowControlThreshold = null,
            SlowConsumingMonitorFlowControlThreshold = null,
            SlowConsumingMonitorPressureWindowSize = null,
        });

        factory.CreateCache("queue-0", NullLoggerFactory.Instance);

        Assert.Empty(strategy.Caches[0].PressureMonitors);
    }

    [Fact]
    public void The_Configured_Pressure_Monitors_Are_Added_To_The_Cache()
    {
        var strategy = new RecordingStrategy();
        var factory = CreateFactory(strategy, new KurrentDBStreamCachePressureOptions
        {
            AveragingCachePressureMonitorFlowControlThreshold = 0.5,
            SlowConsumingMonitorFlowControlThreshold = 0.6,
            SlowConsumingMonitorPressureWindowSize = TimeSpan.FromMinutes(2),
        });

        factory.CreateCache("queue-0", NullLoggerFactory.Instance);

        var monitors = strategy.Caches[0].PressureMonitors;
        Assert.Equal(2, monitors.Count);
        Assert.IsType<AveragingCachePressureMonitor>(monitors[0]);
        Assert.IsType<SlowConsumingPressureMonitor>(monitors[1]);
    }

    [Fact]
    public void The_Catch_Up_Strategy_Builds_A_Cursor_Tracking_Cache()
    {
        var services = new ServiceCollection();
        services.AddOptions<KurrentDBDataAdapterOptions>(ProviderName).Configure(options =>
        {
            var typeMapper = new DefaultTypeMapAttributeMapper();
            typeMapper.RegisterKnownTypes(TypeMapKinds.DomainEvent);
            options.TypeMapper = typeMapper;
            options.GrainStorageSerializer = new FakeSerializer();
        });
        using var provider = services.BuildServiceProvider();
        var strategy = new KurrentDBCatchUpSubscriptionReceiverFactory(provider, ProviderName);
        var factory = CreateFactory(strategy);

        using var cache = factory.CreateCache("queue-0", NullLoggerFactory.Instance);

        Assert.IsType<KurrentDBCatchUpQueueCache>(cache);
        Assert.Equal(KurrentDBCatchUpQueueAdapterReceiver.MaxMessagesPerRead, cache.GetMaxAddCount());
    }
}
