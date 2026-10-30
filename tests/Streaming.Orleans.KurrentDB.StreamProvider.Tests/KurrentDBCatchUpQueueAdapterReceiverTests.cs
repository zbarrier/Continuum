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
///     Verifies the catch-up queue adapter receiver's lifecycle: failed initialization is retried on the next read
///     rather than left half-initialized, and shutdown waits for in-flight work before disposing the cache.
/// </summary>
public class KurrentDBCatchUpQueueAdapterReceiverTests
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

    private static IKurrentDBQueueCache CreateCache(string queue, ILoggerFactory loggerFactory) =>
        CreateCache(queue, loggerFactory, new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(4096), new KurrentDBStreamCacheBufferPoolOptions(), TimeProvider.System));

    private static IKurrentDBQueueCache CreateCache(string queue, ILoggerFactory loggerFactory, KurrentDBQueueCacheBufferPool pool)
    {
        var adapter = new KurrentDBQueueDataAdapter(OrleansSerializer, new KurrentDBDataAdapterOptions
        {
            ConnectionName = "test",
            ProviderName = "test",
            TypeMapper = new FakeTypeMapper(),
            GrainStorageSerializer = new FakeSerializer(),
        });
        var logger = NullLogger.Instance;
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

    /// <summary>
    ///     Initializes only when released, so a test can hold an initialization in flight.
    /// </summary>
    private sealed class GatedReceiver(Task gate) : IKurrentDBReceiver
    {
        public async Task InitAsync() => await gate;

        public Task CloseAsync() => Task.CompletedTask;

        public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount) => [];

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
    }

    [Fact]
    public async Task A_Read_During_Initialization_Does_Not_Start_A_Second_Initialization()
    {
        // Arrange
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var created = 0;
        var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
        var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, CreateCache, NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null,
            (_, _) =>
            {
                Interlocked.Increment(ref created);
                return new GatedReceiver(gate.Task);
            });

        // Act: a read arrives while the explicit initialization is still in flight.
        var initialize = adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        var messages = await adapterReceiver.GetQueueMessagesAsync(10);
        gate.SetResult();
        await initialize;

        // Assert: the read backed off instead of building a second cache and receiver.
        Assert.Empty(messages);
        Assert.Equal(1, created);

        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task A_Receiver_Shut_Down_By_A_Rebalance_Can_Be_Initialized_Again()
    {
        // Arrange
        var created = new List<FlakyReceiver>();
        var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
        var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, CreateCache, NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null,
            (_, _) =>
            {
                var receiver = new FlakyReceiver(failInit: false);
                created.Add(receiver);
                return receiver;
            });
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        Assert.Single(await adapterReceiver.GetQueueMessagesAsync(10));

        // Act: the queue moves away and back, as it does when the pulling agents rebalance.
        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
        Assert.Empty(await adapterReceiver.GetQueueMessagesAsync(10));
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        var messages = await adapterReceiver.GetQueueMessagesAsync(10);

        // Assert: a fresh receiver was built and reads again.
        Assert.Equal(2, created.Count);
        Assert.Single(messages);

        await adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
    }

    /// <summary>
    ///     Closes only when released, so a test can hold a shutdown in flight.
    /// </summary>
    private sealed class GatedCloseReceiver(Task closeGate) : IKurrentDBReceiver
    {
        public Task InitAsync() => Task.CompletedTask;

        public Task CloseAsync() => closeGate;

        public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount) => [];

        public Task MessagesDeliveredAsync(ulong commitPosition) => Task.CompletedTask;
    }

    [Fact]
    public async Task Shutdown_Disposes_The_Cache_Only_After_The_Receiver_Has_Closed()
    {
        // Arrange: a disposed cache unregisters from its pool, which shows when disposal happened.
        var closeGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pool = new KurrentDBQueueCacheBufferPool(() => new FixedSizeBuffer(4096), new KurrentDBStreamCacheBufferPoolOptions(), TimeProvider.System);
        var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
        var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, (queue, loggerFactory) => CreateCache(queue, loggerFactory, pool), NullLoggerFactory.Instance,
            new NullReceiverMonitor(), new LoadSheddingOptions(), null, (_, _) => new GatedCloseReceiver(closeGate.Task));
        await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));
        Assert.Equal(1, pool.ActiveCaches);

        // Act: shutdown starts while the receiver is still closing.
        var shutdown = adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));

        // Assert
        Assert.Equal(1, pool.ActiveCaches);
        closeGate.SetResult();
        await shutdown;
        Assert.Equal(0, pool.ActiveCaches);
    }

	/// <summary>
	///     Holds each checkpoint write open until released, and records whether close overlapped one.
	/// </summary>
	private sealed class GatedDeliveryReceiver(Task deliveryGate) : IKurrentDBReceiver
	{
		private int _deliveriesInFlight;

		public TaskCompletionSource DeliveryStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public bool ClosedDuringDelivery { get; private set; }

		public bool Closed { get; private set; }

		public Task InitAsync() => Task.CompletedTask;

		public Task CloseAsync()
		{
			ClosedDuringDelivery = Volatile.Read(ref _deliveriesInFlight) > 0;
			Closed = true;
			return Task.CompletedTask;
		}

		public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount) => [];

		public async Task MessagesDeliveredAsync(ulong commitPosition)
		{
			Interlocked.Increment(ref _deliveriesInFlight);
			DeliveryStarted.TrySetResult();
			try
			{
				await deliveryGate;
			}
			finally
			{
				Interlocked.Decrement(ref _deliveriesInFlight);
			}
		}
	}

	[Fact]
	public async Task Shutdown_Waits_For_An_In_Flight_Checkpoint_Write_Before_Closing_The_Receiver()
	{
		// Arrange
		var deliveryGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var receiver = new GatedDeliveryReceiver(deliveryGate.Task);
		IKurrentDBCatchUpQueueCache? cache = null;
		var settings = new KurrentDBReceiverSettings { QueueName = "queue" };
		var adapterReceiver = new KurrentDBCatchUpQueueAdapterReceiver(settings, (queue, loggerFactory) => (IKurrentDBQueueCache)(cache = (IKurrentDBCatchUpQueueCache)CreateCache(queue, loggerFactory)),
			NullLoggerFactory.Instance, new NullReceiverMonitor(), new LoadSheddingOptions(), null, (_, _) => receiver);
		await adapterReceiver.Initialize(TimeSpan.FromSeconds(5));

		// Act: a purge reports a checkpointable position, and shutdown starts while its write is in flight.
		cache!.OnCheckpointablePosition!(100);
		await receiver.DeliveryStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
		var shutdown = adapterReceiver.Shutdown(TimeSpan.FromSeconds(5));
		await Task.Delay(200, TestContext.Current.CancellationToken);

		// Assert: close waits for the write, so the final checkpoint it writes is not raced by the earlier one.
		Assert.False(receiver.Closed);
		deliveryGate.SetResult();
		await shutdown;
		Assert.True(receiver.Closed);
		Assert.False(receiver.ClosedDuringDelivery);
	}
}
