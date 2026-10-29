using System.Text;

using Continuum.Streaming;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider;
using Continuum.Orleans.Events;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.TypeMapping;

using Orleans.Storage;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.CatchUp;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Tests for the <c>$all</c> catch-up subscription strategy.
/// </summary>
/// <remarks>
///     These drive <see cref="KurrentDBCatchUpSubscriptionReceiver" /> directly rather than through a silo. The defects this
///     strategy is prone to (resuming from the wrong log position) only appear across a receiver restart, which a
///     shared long lived <see cref="ClusterFixture" /> cluster cannot express. Constructing the receiver directly also
///     avoids the one-queue-per-provider constraint that a second stream provider would impose on the shared cluster.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class CatchUpSubscriptionReceiverTests
{
    private readonly string _connectionString;
    private const string EventType = "catch-up-test-event";

    private readonly ITestOutputHelper _output;

    public CatchUpSubscriptionReceiverTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        // The fixture is depended upon so the KurrentDB container is started, even though the cluster is not used.
        ArgumentNullException.ThrowIfNull(fixture);
        _connectionString = fixture.ConnectionString;
        _output = output;
    }

    [Fact]
    public async Task Resumes_After_Checkpoint_Without_Replaying_Delivered_Events()
    {
        // Arrange
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var streamName = $"catchup-resume-{Guid.NewGuid():N}";
        var settings = CreateSettings(streamName, streamFilterPrefix: streamName);

        await AppendAsync(client, streamName, "first-1", "first-2", "first-3");

        // Act: the first receiver drains the stream and checkpoints what it delivered.
        var firstBatch = await ReceiveAllAsync(client, settings, expectedCount: 3);

        // Events appended while no receiver is running must still be picked up on resume.
        await AppendAsync(client, streamName, "second-1", "second-2");

        var secondBatch = await ReceiveAllAsync(client, settings, expectedCount: 2);

        // Assert
        Assert.Equal(new[] { "first-1", "first-2", "first-3" }, firstBatch);

        // The core assertion: the second receiver resumes from the stored $all commit position, so it sees only the
        // new events. Using a per-stream EventNumber as the resume token would replay the whole log instead.
        Assert.Equal(new[] { "second-1", "second-2" }, secondBatch);
    }

    [Fact]
    public async Task Checkpoints_The_All_Stream_Position_Of_Resolved_Link_Events()
    {
        // Arrange: link events are appended explicitly rather than relying on the $by_category projection, which runs
        // asynchronously and writes to a system stream. A link sits at a different position in $all than the event it
        // points at, which is the divergence this test needs.
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var suffix = Guid.NewGuid().ToString("N");
        var originStream = $"catchup-link-origin-{suffix}";
        var linkStream = $"catchup-link-{suffix}";
        var settings = CreateSettings(linkStream, streamFilterPrefix: linkStream, resolveLinkTos: true);

        // The origin events are written first, so every link points backwards to a lower position in the log. A
        // receiver that checkpointed the resolved event's own position would therefore rewind on resume.
        await AppendAsync(client, originStream, "link-1", "link-2", "link-3");
        await AppendLinksAsync(client, linkStream, originStream, 0, 1);

        // Act
        var firstBatch = await ReceiveAllAsync(client, settings, expectedCount: 2);

        await AppendLinksAsync(client, linkStream, originStream, 2);

        var secondBatch = await ReceiveAllAsync(client, settings, expectedCount: 1);

        // Assert
        Assert.Equal(new[] { "link-1", "link-2" }, firstBatch);

        // Resolved events are delivered, but the checkpoint must track the link's position in $all. Storing the
        // resolved event's own position would resume from before the origin events and replay them here.
        Assert.Equal(new[] { "link-3" }, secondBatch);
    }

    [Fact]
    public async Task Counts_Every_Purged_Event_Towards_The_Checkpoint_Event_Limit()
    {
        // Arrange
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var streamName = "catchup-purge-count-" + Guid.NewGuid().ToString("N");
        var settings = CreateSettings(streamName, streamFilterPrefix: streamName);
        var receiverOptions = (KurrentDBCatchUpSubscriptionReceiverOptions)settings.ReceiverOptions;
        receiverOptions.CheckpointMonitorOptions.MaxEventsBeforeCommit = 3;
        receiverOptions.CheckpointMonitorOptions.MaxTimeBeforeCommit = TimeSpan.FromHours(1);
        var store = new RecordingCheckpointStore();
        receiverOptions.CheckpointStore = store;

        await AppendAsync(client, streamName, "one", "two", "three");

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();
            var received = new List<ulong>();
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (received.Count < 3 && DateTime.UtcNow < deadline)
            {
                received.AddRange(receiver.Receive(10).Where(IsEvent).Select(record => (ulong)record.Cursor.SequenceNumber));
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }
            Assert.Equal(3, received.Count);

            // Act: a cache purge reports only the newest purged position, even when it purged several events.
            await receiver.MessagesDeliveredAsync(received[^1]);

            // Assert: the three purged events reach the threshold, so the checkpoint is stored before the receiver closes.
            Assert.Equal(new[] { received[^1] }, store.Stored);
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Buffers_At_Most_The_Capacity_Until_Orleans_Drains_It()
    {
        // Arrange: more events than the buffer holds, so an unbounded pump would buffer every one of them.
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var streamName = "catchup-bounded-" + Guid.NewGuid().ToString("N");
        var settings = CreateSettings(streamName, streamFilterPrefix: streamName);
        var receiverOptions = (KurrentDBCatchUpSubscriptionReceiverOptions)settings.ReceiverOptions;
        receiverOptions.CheckpointStore = new RecordingCheckpointStore();
        var total = KurrentDBCatchUpSubscriptionReceiver.BufferCapacity + 500;
        foreach (var chunk in Enumerable.Range(0, total).Chunk(1000))
        {
            await AppendAsync(client, streamName, chunk.Select(i => i.ToString()).ToArray());
        }

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();

            // Act: wait for the pump
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (receiver.BufferedCount < KurrentDBCatchUpSubscriptionReceiver.BufferCapacity && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50, TestContext.Current.CancellationToken);
            }
            await Task.Delay(500, TestContext.Current.CancellationToken);

            // Assert: the pump waits at capacity instead of reading the rest of the log into memory.
            Assert.Equal(KurrentDBCatchUpSubscriptionReceiver.BufferCapacity, receiver.BufferedCount);

            // Draining the buffer lets the pump read the remaining events, in order and without loss.
            var received = new List<string>();
            deadline = DateTime.UtcNow.AddSeconds(30);
            while (received.Count < total && DateTime.UtcNow < deadline)
            {
                var batch = receiver.Receive(KurrentDBCatchUpQueueAdapterReceiver.MaxMessagesPerRead);
                received.AddRange(batch.Where(IsEvent).Select(record => Encoding.UTF8.GetString(record.Event!.Data)));
                if (batch.Count == 0)
                {
                    await Task.Delay(50, TestContext.Current.CancellationToken);
                }
            }
            Assert.Equal(Enumerable.Range(0, total).Select(i => i.ToString()), received);
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    private sealed class FailingCheckpointStore : ICheckpointStore<ulong>
    {
        public InvalidOperationException Failure { get; } = new("Checkpoint store is unavailable.");

        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) =>
            Task.FromException<ulong>(Failure);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    [Fact]
    public async Task Init_Failure_Keeps_The_Original_Exception()
    {
        // Arrange: the start position is read from the checkpoint store before subscribing, so a failing store fails
        // initialization without touching KurrentDB.
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings("catchup-init-failure", streamFilterPrefix: "catchup-init-failure");
        var store = new FailingCheckpointStore();
        ((KurrentDBCatchUpSubscriptionReceiverOptions)settings.ReceiverOptions).CheckpointStore = store;
        var receiver = CreateReceiver(client, settings);

        // Act
        var ex = await Assert.ThrowsAsync<KurrentDBStorageException>(receiver.InitAsync);

        // Assert
        Assert.Same(store.Failure, ex.InnerException);
    }

    private sealed class RecordingCheckpointStore : ICheckpointStore<ulong>
    {
        public List<ulong> Stored { get; } = new();

        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(0UL);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default)
        {
            lock (Stored)
            {
                Stored.Add(streamPosition);
            }
            return Task.CompletedTask;
        }
    }

    private static KurrentDBReceiverSettings CreateSettings(string queueName, string streamFilterPrefix, bool resolveLinkTos = false)
    {
        return new KurrentDBReceiverSettings
        {
            Options = new KurrentDBOptions
            {
                Name = $"alltest-{queueName}",
                ConnectionName = Constants.StreamProviderName,
                Queues = new List<string> { queueName },
                Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true },
            },
            ReceiverOptions = new KurrentDBCatchUpSubscriptionReceiverOptions
            {
                // Defaults to true, which would skip everything appended before the first receiver started. These
                // tests append first and subscribe afterwards, so the first run must read from the beginning; the
                // second run is then driven purely by the stored checkpoint.
                StartFromNow = false,
                ResolveLinkTos = resolveLinkTos,
                StreamFilterPrefix = streamFilterPrefix,
                CheckpointMonitorOptions = new KurrentDBCheckpointMonitorOptions
                {
                    // Checkpoint as eagerly as the options allow; these tests assert on resume behaviour, not on
                    // checkpoint throttling.
                    MaxEventsBeforeCommit = 1,
                    MaxTimeBeforeCommit = TimeSpan.FromSeconds(1),
                },
            },
            // The checkpoint key is derived from the connection name and consumer group, so this is what ties the
            // two receivers of a test to the same checkpoint.
            ConsumerGroup = $"alltest-{queueName}",
            QueueName = queueName,
        };
    }

    /// <summary>
    ///     Runs a receiver until <paramref name="expectedCount" /> events arrive, then closes it so the pending
    ///     checkpoint is committed. Returns the payloads in delivery order.
    /// </summary>
    private async Task<List<string>> ReceiveAllAsync(KurrentDBClient client, KurrentDBReceiverSettings settings, int expectedCount)
    {
        var receiverOptions = (KurrentDBCatchUpSubscriptionReceiverOptions)settings.ReceiverOptions;

        // A fresh checkpoint store per receiver mirrors a process restart: nothing is carried in memory, the position
        // has to come back from KurrentDB.
        receiverOptions.CheckpointStore = new KurrentDBCheckpointStore(client);

        var receiver = CreateReceiver(client, settings);
        var payloads = new List<string>();
        try
        {
            await receiver.InitAsync();

            // Poll past the expected count so that an over-delivering receiver (a replay) is detected rather than
            // being cut short at exactly the expected number of events.
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < deadline)
            {
                var received = receiver.Receive(10);
                foreach (var record in received)
                {
                    if (IsEvent(record))
                    {
                        payloads.Add(Encoding.UTF8.GetString(record.Event!.Data));
                    }
                    // In a silo the cache raises this once the event has been purged behind every cursor. Here the
                    // test is the consumer, so it reports delivery itself; without it nothing would ever checkpoint.
                    await receiver.MessagesDeliveredAsync((ulong)record.Cursor.SequenceNumber);
                }
                if (payloads.Count >= expectedCount)
                {
                    // Give a replaying receiver a chance to hand over the extra events it should not have.
                    await Task.Delay(500);
                    foreach (var record in receiver.Receive(10))
                    {
                        if (IsEvent(record))
                        {
                            payloads.Add(Encoding.UTF8.GetString(record.Event!.Data));
                        }
                        await receiver.MessagesDeliveredAsync((ulong)record.Cursor.SequenceNumber);
                    }
                    break;
                }
                await Task.Delay(100);
            }
        }
        finally
        {
            await receiver.CloseAsync();
        }
        _output.WriteLine($"Received {payloads.Count} event(s): {string.Join(", ", payloads)}");
        return payloads;
    }

    private KurrentDBCatchUpSubscriptionReceiver CreateReceiver(KurrentDBClient client, KurrentDBReceiverSettings settings) =>
        new(client, settings, new TestOutputLogger(_output), CheckpointMarkerData);

    private static readonly KurrentDBCheckpointMarkerData CheckpointMarkerData = KurrentDBCheckpointMarkerData.Create(new KurrentDBDataAdapterOptions
    {
        TypeMapper = CreateMarkerTypeMapper(),
        GrainStorageSerializer = new EmptySerializer(),
    });

    private static TypeMapper CreateMarkerTypeMapper()
    {
        var typeMapper = new MarkerTypeMapper();
        typeMapper.AddType(typeof(CheckpointMarker), CheckpointMarker.TypeName);
        return typeMapper;
    }

    // Filtered subscriptions interleave checkpoint markers with the events, which these tests do not assert on.
    private static bool IsEvent(StreamedEvent<KurrentDBRawEvent> record) =>
        record.StreamName.Value != KurrentDBStreamedEventConversions.CheckpointStreamName.Value;

    private sealed class MarkerTypeMapper : TypeMapper;

    private sealed class EmptySerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => BinaryData.FromBytes(Array.Empty<byte>());

        public T Deserialize<T>(BinaryData input) => throw new NotSupportedException();
    }

    private static Task AppendAsync(KurrentDBClient client, string streamName, params string[] payloads)
    {
        var events = payloads.Select(payload => new EventData(Uuid.NewUuid(), EventType, Encoding.UTF8.GetBytes(payload)));
        return client.AppendToStreamAsync(streamName, StreamState.Any, events);
    }

    /// <summary>
    ///     Appends KurrentDB link events (<c>$&gt;</c>) pointing at the given event numbers of the origin stream. This
    ///     is the same shape of event the standard projections emit, without depending on them running.
    /// </summary>
    private static Task AppendLinksAsync(KurrentDBClient client, string linkStream, string originStream, params int[] originEventNumbers)
    {
        var links = originEventNumbers.Select(eventNumber =>
            new EventData(Uuid.NewUuid(), "$>", Encoding.UTF8.GetBytes($"{eventNumber}@{originStream}")));
        return client.AppendToStreamAsync(linkStream, StreamState.Any, links);
    }

    /// <summary>
    ///     Forwards receiver logs to the test output, so a subscription that fails inside the background pump is
    ///     visible instead of surfacing only as "no events received".
    /// </summary>
    private sealed class TestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper _output;

        public TestOutputLogger(ITestOutputHelper output) => _output = output;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _output.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception is not null)
            {
                _output.WriteLine(exception.ToString());
            }
        }
    }
}
