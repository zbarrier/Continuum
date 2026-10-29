using Continuum.Orleans.Streams;
using Continuum.TypeMapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Serialization;
using Orleans.Storage;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Verifies that a receiver whose initialization fails is retried on the next read rather than left in place
///     half-initialized.
/// </summary>
public class KurrentDBCatchUpQueueAdapterReceiverInitializationTests
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
    ///     Fails its first initialization, then succeeds and hands out a single event.
    /// </summary>
    private sealed class FlakyReceiver(bool failInit) : IKurrentDBReceiver
    {
        private bool _delivered;

        public bool Initialized { get; private set; }

        public Task InitAsync()
        {
            if (failInit)
            {
                throw new InvalidOperationException("KurrentDB is unavailable.");
            }
            Initialized = true;
            return Task.CompletedTask;
        }

        public Task CloseAsync() => Task.CompletedTask;

        public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("Receive was called on a receiver that never initialized.");
            }
            if (_delivered)
            {
                return [];
            }
            _delivered = true;
            return [new(Guid.NewGuid(), "test-event", Stream, 0, new StreamedEventCursor("test", Stream.Category, Stream.Value, 100, 0), DateTime.UtcNow, new KurrentDBRawEvent(new byte[16]))];
        }

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
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
        var evictionStrategy = new KurrentDBCatchUpEvictionStrategy(logger, new TimePurgePredicate(TimeSpan.FromHours(1), TimeSpan.FromHours(2)), null, null, adapter, cursorTracker);
        return new KurrentDBCatchUpQueueCache(queue, 100, pool, adapter, evictionStrategy, cursorTracker, logger, null!, null, null);
    }

    [Fact]
    public async Task Failed_Initialization_Is_Retried_On_The_Next_Read()
    {
        // Arrange
        var created = new List<FlakyReceiver>();
        var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
        var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, CreateCache, NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null,
            (_, _) =>
            {
                var receiver = new FlakyReceiver(failInit: created.Count == 0);
                created.Add(receiver);
                return receiver;
            });

        // Act: the first initialization fails, as it would when KurrentDB is unreachable at silo start.
        await Assert.ThrowsAsync<InvalidOperationException>(() => adapterReceiver.Initialize(TimeSpan.FromSeconds(5)));
        var messages = await adapterReceiver.GetQueueMessagesAsync(10);

        // Assert: the read rebuilt the receiver instead of reading from the one that never initialized.
        Assert.Equal(2, created.Count);
        Assert.True(created[1].Initialized);
        Assert.Single(messages);

        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
    }
}
