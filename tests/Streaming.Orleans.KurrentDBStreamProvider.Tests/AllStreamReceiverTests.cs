using System.Text;

using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.KurrentDB;

namespace Continuum.Streaming.Orleans.KurrentDBStreamProvider.Tests;

/// <summary>
///     Tests for the <c>$all</c> catch-up subscription strategy.
/// </summary>
/// <remarks>
///     These drive <see cref="KurrentDBAllStreamReceiver" /> directly rather than through a silo. The defects this
///     strategy is prone to (resuming from the wrong log position) only appear across a receiver restart, which a
///     shared long lived <see cref="ClusterFixture" /> cluster cannot express. Constructing the receiver directly also
///     avoids the one-queue-per-provider constraint that a second stream provider would impose on the shared cluster.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class AllStreamReceiverTests
{
    private const string ConnectionString = "kurrentdb://localhost:2113?tls=false";
    private const string EventType = "all-stream-test-event";

    private readonly ITestOutputHelper _output;

    public AllStreamReceiverTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        // The fixture is depended upon so the KurrentDB container is started, even though the cluster is not used.
        ArgumentNullException.ThrowIfNull(fixture);
        _output = output;
    }

    [Fact]
    public async Task Resumes_After_Checkpoint_Without_Replaying_Delivered_Events()
    {
        // Arrange
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var streamName = $"allstream-resume-{Guid.NewGuid():N}";
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
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(ConnectionString));
        var suffix = Guid.NewGuid().ToString("N");
        var originStream = $"allstream-link-origin-{suffix}";
        var linkStream = $"allstream-link-{suffix}";
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

    private static KurrentDBReceiverSettings CreateSettings(string queueName, string streamFilterPrefix, bool resolveLinkTos = false)
    {
        return new KurrentDBReceiverSettings
        {
            Options = new KurrentDBOptions
            {
                // The subscription name, and therefore the checkpoint stream, is derived from these two values, so
                // they are what ties the two receivers of a test to the same checkpoint.
                Name = $"alltest-{queueName}",
                ConnectionName = Constants.StreamProviderName,
                Queues = new List<string> { queueName },
                Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true },
            },
            ReceiverOptions = new KurrentDBAllStreamReceiverOptions
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
            ConsumerGroup = "unused-for-all-stream",
            QueueName = queueName,
        };
    }

    /// <summary>
    ///     Runs a receiver until <paramref name="expectedCount" /> events arrive, then closes it so the pending
    ///     checkpoint is committed. Returns the payloads in delivery order.
    /// </summary>
    private async Task<List<string>> ReceiveAllAsync(KurrentDBClient client, KurrentDBReceiverSettings settings, int expectedCount)
    {
        var receiverOptions = (KurrentDBAllStreamReceiverOptions)settings.ReceiverOptions;

        // A fresh checkpoint store per receiver mirrors a process restart: nothing is carried in memory, the position
        // has to come back from KurrentDB.
        receiverOptions.CheckpointStore = new KurrentDBCheckpointStore(client);

        var receiver = new KurrentDBAllStreamReceiver(client, settings, string.Empty, new TestOutputLogger(_output));
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
                    payloads.Add(Encoding.UTF8.GetString(record.Event!.Data));
                    // In a silo the cache raises this once the event has been purged behind every cursor. Here the
                    // test is the consumer, so it reports delivery itself; without it nothing would ever checkpoint.
                    await receiver.MessagesDeliveredAsync((ulong)record.SequenceNumber);
                }
                if (payloads.Count >= expectedCount)
                {
                    // Give a replaying receiver a chance to hand over the extra events it should not have.
                    await Task.Delay(500);
                    foreach (var record in receiver.Receive(10))
                    {
                        payloads.Add(Encoding.UTF8.GetString(record.Event!.Data));
                        await receiver.MessagesDeliveredAsync((ulong)record.SequenceNumber);
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
