using System.Diagnostics.CodeAnalysis;

using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies the persistent queue adapter receiver's lifecycle (re-initialization after a rebalance, and closing the
///     receiver before its cache is disposed) and the persistent cache's pressure calculation.
/// </summary>
public class KurrentDBPersistentQueueAdapterReceiverTests
{
    private const int MaxAddCount = 1;

    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class NullReceiverMonitor : IQueueAdapterReceiverMonitor
    {
        public void TrackInitialization(bool success, TimeSpan callTime, Exception? exception) { }

        public void TrackRead(bool success, TimeSpan callTime, Exception? exception) { }

        public void TrackMessagesReceived(long count, DateTime? oldestMessageEnqueueTimeUtc, DateTime? newestMessageEnqueueTimeUtc) { }

        public void TrackShutdown(bool success, TimeSpan callTime, Exception? exception) { }
    }

    private sealed class RecordingMonitor : ICachePressureMonitor
    {
        public List<double> Contributions { get; } = [];

        public ICacheMonitor CacheMonitor { set { } }

        public void RecordCachePressureContribution(double cachePressureContribution) => Contributions.Add(cachePressureContribution);

        public bool IsUnderPressure(DateTime utcNow) => false;
    }

    /// <summary>
    ///     Hands out a single event, and holds <see cref="CloseAsync" /> open until <paramref name="closeGate" /> completes.
    /// </summary>
    private sealed class FakeReceiver(Task closeGate) : IKurrentDBReceiver
    {
        private bool _delivered;

        public Task InitAsync() => Task.CompletedTask;

        public Task CloseAsync() => closeGate;

        public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
        {
            if (_delivered)
            {
                return [];
            }
            _delivered = true;
            return [CreateEvent(0, 100)];
        }

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
    }

    /// <summary>
    ///     Records when the cache was disposed so a test can check it happened after the receiver closed.
    /// </summary>
    private sealed class TrackingCache(IKurrentDBQueueCache inner) : IKurrentDBQueueCache
    {
        public bool Disposed { get; private set; }

        public List<StreamPosition> Add(List<StreamedEvent<KurrentDBRawEvent>> queueMessages, DateTime dequeueTimeUtc) => inner.Add(queueMessages, dequeueTimeUtc);

        public object GetCursor(StreamId streamId, StreamSequenceToken? sequenceToken) => inner.GetCursor(streamId, sequenceToken);

        public object GetCursorAtPosition(StreamId streamId, StreamSubscriptionStartPosition startPosition) => inner.GetCursorAtPosition(streamId, startPosition);

        public bool TryGetNextMessage(object cursorObj, [MaybeNullWhen(false)] out IBatchContainer container) => inner.TryGetNextMessage(cursorObj, out container);

        public void AddCachePressureMonitor(ICachePressureMonitor monitor) => inner.AddCachePressureMonitor(monitor);

        public void SignalPurge() => inner.SignalPurge();

        public int GetMaxAddCount() => inner.GetMaxAddCount();

        public void Dispose()
        {
            Disposed = true;
            inner.Dispose();
        }
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong streamVersion, ulong commitPosition) =>
        new(Guid.NewGuid(), "test-event", Stream, streamVersion, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0),
            DateTime.UtcNow, new KurrentDBRawEvent([1]));

    private static KurrentDBPersistentQueueCache CreatePersistentCache(string queue, int maxAddCount = 100)
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
        var pool = new ObjectPool<FixedSizeBuffer>(() => new FixedSizeBuffer(1 << 20));
        var evictionStrategy = new KurrentDBPersistentEvictionStrategy(logger, new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2)), null, null);
        return new KurrentDBPersistentQueueCache(queue, maxAddCount, pool, adapter, evictionStrategy, logger, null!, null, null);
    }

    private static KurrentDBPersistentQueueAdapterReceiver CreateAdapterReceiver(Func<string, ILoggerFactory, IKurrentDBQueueCache> cacheFactory, Func<IKurrentDBReceiver> receiverFactory) =>
        new(new KurrentDBReceiverSettings { QueueName = "queue" }, cacheFactory, NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null,
            (_, _) => receiverFactory());

    [Fact]
    public async Task A_Receiver_Shut_Down_By_A_Rebalance_Can_Be_Initialized_Again()
    {
        // Arrange
        var created = 0;
        var adapterReceiver = CreateAdapterReceiver((queue, _) => CreatePersistentCache(queue), () =>
        {
            created++;
            return new FakeReceiver(Task.CompletedTask);
        });
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        Assert.Single(await adapterReceiver.GetQueueMessagesAsync(10));

        // Act: the queue moves away and back, as it does when the pulling agents rebalance.
        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
        Assert.Empty(await adapterReceiver.GetQueueMessagesAsync(10));
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        var messages = await adapterReceiver.GetQueueMessagesAsync(10);

        // Assert: a fresh receiver was built and reads again.
        Assert.Equal(2, created);
        Assert.Single(messages);

        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Shutdown_Disposes_The_Cache_Only_After_The_Receiver_Has_Closed()
    {
        // Arrange
        var closeGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        TrackingCache? cache = null;
        var adapterReceiver = CreateAdapterReceiver((queue, _) => cache = new TrackingCache(CreatePersistentCache(queue)), () => new FakeReceiver(closeGate.Task));
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));

        // Act: shutdown starts while the receiver is still closing.
        var shutdown = adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));

        // Assert
        Assert.False(cache!.Disposed);
        closeGate.SetResult();
        await shutdown;
        Assert.True(cache.Disposed);
    }

    [Fact]
    public void A_Few_Events_Far_Apart_In_The_Log_Do_Not_Measure_Pressure()
    {
        // Commit positions are byte offsets, so a few events can span far more than the item threshold.
        var contributions = ReadFirstContribution(10 * MaxAddCount - 1);

        Assert.Equal([0.0], contributions);
    }

    [Fact]
    public void Enough_Events_Measure_Pressure_From_The_Cursor_Position()
    {
        var contributions = ReadFirstContribution(10 * MaxAddCount + 1);

        // The oldest event is the furthest from the newest, so it is the most at risk.
        Assert.Equal([1.0], contributions);
    }

    private static List<double> ReadFirstContribution(int count)
    {
        var cache = CreatePersistentCache("queue", MaxAddCount);
        var monitor = new RecordingMonitor();
        cache.AddCachePressureMonitor(monitor);
        var positions = cache.Add(Enumerable.Range(0, count).Select(i => CreateEvent((ulong)i, (ulong)(i + 1) * 1_000)).ToList(), DateTime.UtcNow);
        var cursor = cache.GetCursor(positions[0].StreamId, positions[0].SequenceToken);
        Assert.True(cache.TryGetNextMessage(cursor, out _));
        return monitor.Contributions;
    }
}
