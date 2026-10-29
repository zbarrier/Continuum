using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Statistics;
using Orleans.Storage;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies how the adapter receiver sheds load when the silo is under pressure and how it reports failures while
///     reading from KurrentDB.
/// </summary>
public class KurrentDBQueueAdapterReceiverReadTests
{
    private static readonly Serializer OrleansSerializer = new ServiceCollection().AddSerializer().BuildServiceProvider().GetRequiredService<Serializer>();

    private static readonly StreamName Stream = new("test-1", "test", null, "1");

    private sealed class FakeTypeMapper : TypeMapper;

    private sealed class FakeSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private sealed class RecordingReceiverMonitor : IQueueAdapterReceiverMonitor
    {
        public List<(bool Success, Exception? Exception)> Reads { get; } = [];

        public List<long> MessagesReceived { get; } = [];

        public void TrackInitialization(bool success, TimeSpan callTime, Exception? exception) { }

        public void TrackRead(bool success, TimeSpan callTime, Exception? exception) => Reads.Add((success, exception));

        public void TrackMessagesReceived(long count, DateTime? oldestMessageEnqueueTimeUtc, DateTime? newestMessageEnqueueTimeUtc) => MessagesReceived.Add(count);

        public void TrackShutdown(bool success, TimeSpan callTime, Exception? exception) { }
    }

    private sealed class FakeReceiver(Func<int, List<StreamedEvent<KurrentDBRawEvent>>> receive) : IKurrentDBReceiver
    {
        public Task InitAsync() => Task.CompletedTask;

        public Task CloseAsync() => Task.CompletedTask;

        public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount) => receive(maxCount);

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
    }

    private sealed class FakeEnvironmentStatisticsProvider(float cpuUsagePercentage) : IEnvironmentStatisticsProvider
    {
        public EnvironmentStatistics GetEnvironmentStatistics() =>
            (EnvironmentStatistics)Activator.CreateInstance(typeof(EnvironmentStatistics), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null,
                [cpuUsagePercentage, cpuUsagePercentage, 1_000L, 1_000L, 1_000_000L, 1_000_000L, 2_000_000L], null)!;
    }

    private static IKurrentDBQueueCache CreateCache(string queue, ILoggerFactory loggerFactory)
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
        var pool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(4096), new KurrentDBStreamCacheBufferPoolOptions(), TimeProvider.System);
        var cursorTracker = new KurrentDBCursorTracker();
        var evictionStrategy = new KurrentDBEvictionStrategy(logger, new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2)), null, null, adapter, cursorTracker);
        return new KurrentDBQueueCache(queue, 100, pool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    private static StreamedEvent<KurrentDBRawEvent> CreateEvent(ulong commitPosition) =>
        new(Guid.NewGuid(), "test-event", Stream, 0, new StreamedEventCursor("test", Stream.Category, Stream.Value, commitPosition, 0), DateTime.UtcNow, new KurrentDBRawEvent(new byte[16]));

    private static KurrentDBQueueAdapterReceiver CreateReceiver(IKurrentDBReceiver receiver, IQueueAdapterReceiverMonitor? monitor = null, LoadSheddingOptions? loadSheddingOptions = null, IEnvironmentStatisticsProvider? statistics = null) =>
        new(new KurrentDBReceiverSettings { QueueName = "queue" }, CreateCache, NullLoggerFactory.Instance, monitor ?? new RecordingReceiverMonitor(),
            loadSheddingOptions ?? new LoadSheddingOptions(), statistics, (_, _) => receiver);

    [Fact]
    public async Task Stops_Accepting_Events_When_The_Silo_Is_Over_Its_Load_Shedding_Limit()
    {
        var receiver = CreateReceiver(new FakeReceiver(_ => []), loadSheddingOptions: new LoadSheddingOptions { LoadSheddingEnabled = true, CpuThreshold = 80 },
            statistics: new FakeEnvironmentStatisticsProvider(99));
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.Equal(0, receiver.GetMaxAddCount());

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Accepts_Events_When_The_Silo_Is_Under_Its_Load_Shedding_Limit()
    {
        var receiver = CreateReceiver(new FakeReceiver(_ => []), loadSheddingOptions: new LoadSheddingOptions { LoadSheddingEnabled = true, CpuThreshold = 80 },
            statistics: new FakeEnvironmentStatisticsProvider(10));
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.True(receiver.GetMaxAddCount() > 0);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Does_Not_Shed_Load_When_Load_Shedding_Is_Disabled()
    {
        var receiver = CreateReceiver(new FakeReceiver(_ => []), loadSheddingOptions: new LoadSheddingOptions { LoadSheddingEnabled = false },
            statistics: new FakeEnvironmentStatisticsProvider(99));
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.True(receiver.GetMaxAddCount() > 0);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Does_Not_Shed_Load_Without_Environment_Statistics()
    {
        var receiver = CreateReceiver(new FakeReceiver(_ => []), loadSheddingOptions: new LoadSheddingOptions { LoadSheddingEnabled = true, CpuThreshold = 80 });
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.True(receiver.GetMaxAddCount() > 0);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Rethrows_And_Reports_A_Failed_Read()
    {
        var monitor = new RecordingReceiverMonitor();
        var failure = new InvalidOperationException("KurrentDB read failed.");
        var receiver = CreateReceiver(new FakeReceiver(_ => throw failure), monitor);
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => receiver.GetQueueMessagesAsync(10));

        Assert.Same(failure, exception);
        var read = Assert.Single(monitor.Reads);
        Assert.False(read.Success);
        Assert.Same(failure, read.Exception);
        Assert.Empty(monitor.MessagesReceived);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Keeps_Reading_After_A_Failed_Read()
    {
        var monitor = new RecordingReceiverMonitor();
        var calls = 0;
        var receiver = CreateReceiver(new FakeReceiver(_ => ++calls == 1 ? throw new InvalidOperationException("Transient.") : [CreateEvent(100)]), monitor);
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        await Assert.ThrowsAsync<InvalidOperationException>(() => receiver.GetQueueMessagesAsync(10));
        var messages = await receiver.GetQueueMessagesAsync(10);

        Assert.Single(messages);
        Assert.Equal([false, true], monitor.Reads.Select(read => read.Success));
        Assert.Equal([1L], monitor.MessagesReceived);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Reports_An_Empty_Read()
    {
        var monitor = new RecordingReceiverMonitor();
        var receiver = CreateReceiver(new FakeReceiver(_ => []), monitor);
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.Empty(await receiver.GetQueueMessagesAsync(10));

        Assert.Equal([0L], monitor.MessagesReceived);
        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Returns_Nothing_Without_Reading_After_Shutdown()
    {
        var calls = 0;
        var receiver = CreateReceiver(new FakeReceiver(_ => { calls++; return [CreateEvent(100)]; }));
        await receiver.Initialize(TimeSpan.FromSeconds(5));
        await receiver.Shutdown(TimeSpan.FromSeconds(5));

        Assert.Empty(await receiver.GetQueueMessagesAsync(10));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task Returns_Nothing_Without_Reading_For_A_Non_Positive_Count()
    {
        var calls = 0;
        var receiver = CreateReceiver(new FakeReceiver(_ => { calls++; return [CreateEvent(100)]; }));
        await receiver.Initialize(TimeSpan.FromSeconds(5));

        Assert.Empty(await receiver.GetQueueMessagesAsync(0));
        Assert.Equal(0, calls);

        await receiver.Shutdown(TimeSpan.FromSeconds(5));
    }
}
