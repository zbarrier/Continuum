using System.Text;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

using Orleans.Configuration;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Tests;

/// <summary>
///     Tests for the persistent subscription strategy against a real KurrentDB node.
/// </summary>
/// <remarks>
///     These drive <see cref="KurrentDBPersistentSubscriptionReceiver" /> directly, so subscription setup, server side
///     drops and redeliveries can be controlled per test without disturbing the shared cluster.
/// </remarks>
[Collection(ClusterCollection.Name)]
public class KurrentDBPersistentSubscriptionReceiverTests
{
    private const string EventType = "persistent-test-event";

    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;

    public KurrentDBPersistentSubscriptionReceiverTests(ClusterFixture fixture, ITestOutputHelper output)
    {
        // The fixture is depended upon so the KurrentDB container is started, even though the cluster is not used.
        ArgumentNullException.ThrowIfNull(fixture);
        _connectionString = fixture.ConnectionString;
        _output = output;
    }

    [Fact]
    public async Task Creates_The_Consumer_Group_And_Delivers_Events()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings();
        await AppendAsync(writer, settings.QueueName, "one", "two");

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();

            var info = await client.GetInfoToStreamAsync(settings.QueueName, settings.ConsumerGroup, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(StreamPosition.Start, info.Settings!.StartFrom);
            Assert.Equal(["one", "two"], await ReceiveAsync(receiver, 2));
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Keeps_An_Existing_Consumer_Group_Unless_Update_Is_Requested()
    {
        await using var client = CreateSubscriptionsClient();
        var settings = CreateSettings(startFromNow: true);
        await RunReceiverAsync(client, settings);

        var resumed = CreateSettings(settings.QueueName, startFromNow: false);
        await RunReceiverAsync(client, resumed);

        var info = await client.GetInfoToStreamAsync(settings.QueueName, settings.ConsumerGroup, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(StreamPosition.End, info.Settings!.StartFrom);
    }

    [Fact]
    public async Task Updates_An_Existing_Consumer_Group_When_Requested()
    {
        await using var client = CreateSubscriptionsClient();
        var settings = CreateSettings(startFromNow: true);
        await RunReceiverAsync(client, settings);

        var updated = CreateSettings(settings.QueueName, startFromNow: false, updateExisting: true);
        await RunReceiverAsync(client, updated);

        var info = await client.GetInfoToStreamAsync(settings.QueueName, settings.ConsumerGroup, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(StreamPosition.Start, info.Settings!.StartFrom);
    }

    [Fact]
    public async Task Filters_Events_Redelivered_Before_They_Are_Received()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        // A short message timeout makes the server redeliver events that sit unacknowledged in the internal queue.
        var settings = CreateSettings(messageTimeout: TimeSpan.FromMilliseconds(500));
        await AppendAsync(writer, settings.QueueName, "only-once");

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();

            // Wait long enough for several redeliveries before draining the internal queue.
            await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);

            Assert.Equal(["only-once"], await ReceiveAsync(receiver, 1));
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Acknowledges_Events_As_They_Are_Received()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings();
        await AppendAsync(writer, settings.QueueName, "acked-1", "acked-2");

        Assert.Equal(["acked-1", "acked-2"], await RunReceiverAsync(client, settings, expectedCount: 2));

        // A new receiver on the same consumer group must not be handed the acknowledged events again.
        await AppendAsync(writer, settings.QueueName, "later");
        Assert.Equal(["later"], await RunReceiverAsync(client, settings, expectedCount: 1));
    }

    [Fact]
    public async Task Resubscribes_After_The_Server_Drops_The_Subscription()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings();

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();
            await client.RestartSubsystemAsync(cancellationToken: TestContext.Current.CancellationToken);

            await AppendAsync(writer, settings.QueueName, "after-drop");
            Assert.Equal(["after-drop"], await ReceiveAsync(receiver, 1));
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Resubscribes_After_Repeated_Drops_With_A_Single_Connection()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings();

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();
            // The server rejects a restart while one is in progress, so the drops are issued back to back instead.
            await client.RestartSubsystemAsync(cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(1, await WaitForConnectionCountAsync(client, settings, expected: 1));
            await client.RestartSubsystemAsync(cancellationToken: TestContext.Current.CancellationToken);

            await AppendAsync(writer, settings.QueueName, "after-drops");
            Assert.Equal(["after-drops"], await ReceiveAsync(receiver, 1));
            Assert.Equal(1, await WaitForConnectionCountAsync(client, settings, expected: 1));
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Close_Disconnects_And_Stops_Receiving()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        var settings = CreateSettings();

        var receiver = CreateReceiver(client, settings);
        await receiver.InitAsync();
        Assert.Equal(1, await WaitForConnectionCountAsync(client, settings, expected: 1));

        await receiver.CloseAsync();
        await receiver.CloseAsync();

        Assert.Equal(0, await WaitForConnectionCountAsync(client, settings, expected: 0));
        await AppendAsync(writer, settings.QueueName, "after-close");
        await Task.Delay(500, TestContext.Current.CancellationToken);
        Assert.Empty(receiver.Receive(10));
    }

    [Fact]
    public async Task Received_Events_Are_No_Longer_Tracked_For_Duplicates()
    {
        await using var client = CreateSubscriptionsClient();
        await using var writer = new KurrentDBClient(KurrentDBClientSettings.Create(_connectionString));
        // A short message timeout makes the server redeliver events while they sit in the internal queue.
        var settings = CreateSettings(messageTimeout: TimeSpan.FromMilliseconds(500));
        await AppendAsync(writer, settings.QueueName, "a", "b", "c");

        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();
            await Task.Delay(TimeSpan.FromSeconds(3), TestContext.Current.CancellationToken);
            Assert.Equal(3, receiver.TrackedEventIdCount);

            Assert.Equal(["a", "b", "c"], await ReceiveAsync(receiver, 3));

            // Every ID was released, so the set does not grow and a later delivery of the event is not suppressed.
            Assert.Equal(0, receiver.TrackedEventIdCount);
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    [Fact]
    public async Task Receive_Before_Init_Returns_Nothing()
    {
        await using var client = CreateSubscriptionsClient();
        var receiver = CreateReceiver(client, CreateSettings());

        Assert.Empty(receiver.Receive(10));
    }

    private KurrentDBPersistentSubscriptionsClient CreateSubscriptionsClient() =>
        new(KurrentDBClientSettings.Create(_connectionString));

    private KurrentDBPersistentSubscriptionReceiver CreateReceiver(KurrentDBPersistentSubscriptionsClient client, KurrentDBReceiverSettings settings) =>
        new(client, settings, new TestOutputLogger(_output));

    private static KurrentDBReceiverSettings CreateSettings(string? queueName = null, bool startFromNow = false, bool updateExisting = false, TimeSpan? messageTimeout = null)
    {
        queueName ??= $"persistent-{Guid.NewGuid():N}";
        return new KurrentDBReceiverSettings
        {
            Options = new KurrentDBOptions
            {
                Name = $"persistenttest-{queueName}",
                ConnectionName = Constants.StreamProviderName,
                Queues = [queueName],
                Credentials = new KurrentDBStreamCredentialsOptions { UseDefault = true },
            },
            ReceiverOptions = new KurrentDBPersistentSubscriptionReceiverOptions
            {
                StartFromNow = startFromNow,
                UpdateExistingSubscription = updateExisting,
                SubscriptionSettings = new PersistentSubscriptionSettings(messageTimeout: messageTimeout ?? TimeSpan.FromSeconds(30), maxRetryCount: 100),
            },
            ConsumerGroup = $"persistenttest-{queueName}",
            QueueName = queueName,
        };
    }

    /// <summary>
    ///     Runs a receiver until <paramref name="expectedCount" /> events arrive, then closes it.
    /// </summary>
    private async Task<List<string>> RunReceiverAsync(KurrentDBPersistentSubscriptionsClient client, KurrentDBReceiverSettings settings, int expectedCount = 0)
    {
        var receiver = CreateReceiver(client, settings);
        try
        {
            await receiver.InitAsync();
            return await ReceiveAsync(receiver, expectedCount);
        }
        finally
        {
            await receiver.CloseAsync();
        }
    }

    /// <summary>
    ///     Polls until <paramref name="expectedCount" /> events arrive, then keeps polling briefly so an extra
    ///     (duplicate or replayed) delivery is detected instead of being cut short.
    /// </summary>
    private static async Task<List<string>> ReceiveAsync(KurrentDBPersistentSubscriptionReceiver receiver, int expectedCount)
    {
        var payloads = new List<string>();
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (payloads.Count < expectedCount && DateTime.UtcNow < deadline)
        {
            payloads.AddRange(receiver.Receive(10).Select(e => Encoding.UTF8.GetString(e.Event!.Data)));
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        await Task.Delay(500, TestContext.Current.CancellationToken);
        payloads.AddRange(receiver.Receive(10).Select(e => Encoding.UTF8.GetString(e.Event!.Data)));
        return payloads;
    }

    private static async Task<int> WaitForConnectionCountAsync(KurrentDBPersistentSubscriptionsClient client, KurrentDBReceiverSettings settings, int expected)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (true)
        {
            var count = (await client.GetInfoToStreamAsync(settings.QueueName, settings.ConsumerGroup, cancellationToken: TestContext.Current.CancellationToken)).Connections.Count();
            if (count == expected || DateTime.UtcNow >= deadline)
            {
                return count;
            }
            await Task.Delay(200, TestContext.Current.CancellationToken);
        }
    }

    private static Task AppendAsync(KurrentDBClient client, string streamName, params string[] payloads)
    {
        var events = payloads.Select(payload => new EventData(Uuid.NewUuid(), EventType, Encoding.UTF8.GetBytes(payload)));
        return client.AppendToStreamAsync(streamName, StreamState.Any, events, cancellationToken: TestContext.Current.CancellationToken);
    }

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
