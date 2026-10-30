using Continuum.Orleans.Streams;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Orleans.Streams;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a shutdown landing while a read is inside the receiver drops the read's events instead of adding
///     them to the cache the shutdown disposed.
/// </summary>
public class ShutdownDuringReadTests
{
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

    /// <summary>
    ///     Starts a shutdown from inside <see cref="Receive" />, so the read returns events after the cache was cleared.
    /// </summary>
    private sealed class ShutdownDuringReceiveReceiver : IKurrentDBReceiver
    {
        public Func<Task>? OnReceive { get; set; }

        public Task? Shutdown { get; private set; }

        public Task InitAsync() => Task.CompletedTask;

        public Task CloseAsync() => Task.CompletedTask;

        public IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
        {
            Shutdown = OnReceive!();
            return [new(Guid.NewGuid(), "test-event", Stream, 0, new StreamedEventCursor("test", Stream.Category, Stream.Value, 100, 0), DateTime.UtcNow, new KurrentDBRawEvent(new byte[16]))];
        }

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
    }

    private static IKurrentDBQueueCache CreateCache(string queue, ILoggerFactory loggerFactory)
    {
        var pool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(4096), new KurrentDBStreamCacheBufferPoolOptions(), TimeProvider.System);
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var cursorTracker = new KurrentDBCursorTracker();
        var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(NullLogger.Instance, new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2)), null, null, adapter, cursorTracker);
        return new KurrentDBCatchUpQueueCache(queue, 100, pool, adapter, evictionStrategy, cursorTracker, NullLogger.Instance, null!, null, null);
    }

    [Fact]
    public async Task A_Shutdown_During_A_Read_Drops_The_Events_Instead_Of_Adding_Them_To_The_Disposed_Cache()
    {
        // Arrange
        var receiver = new ShutdownDuringReceiveReceiver();
        var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
        var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, CreateCache, NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null, (_, _) => receiver);
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        receiver.OnReceive = () => adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));

        // Act: shutdown clears and disposes the cache while the read is still inside the receiver.
        var messages = await adapterReceiver.GetQueueMessagesAsync(10);
        await receiver.Shutdown!;

        // Assert: the read sees the cleared cache and returns nothing; the events are replayed after the next init.
        Assert.Empty(messages);
    }
}
