using System.Collections.Concurrent;
using System.Text.Json;

using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.Streams;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Tests.Integration;

/// <summary>
/// Runs catch-up subscriptions against the Cosmos DB emulator to verify delivery, retry of a failed batch with the
/// matching health and state, resuming from the lease container after a restart, and that disabled subscriptions do not run.
/// </summary>
[Collection(CosmosDBCollection.Name)]
public class CosmosDBCatchupSubscriptionIntegrationTests(CosmosDBFixture fixture)
{
    private const string ConnectionName = "cosmosdb";
    private const string EventType = "test-event";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Delivers_events_in_order_and_reports_running_then_stopped()
    {
        var containers = await fixture.CreateContainersAsync();
        await using var harness = CreateHarness(containers);
        await WriteAsync(containers.Monitored, NewStream(), 0, 3);

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 3);

        Assert.Equal(["event-0", "event-1", "event-2"], harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal(CosmosDBCatchupSubscriptionState.Running, harness.State(FirstCatchupSubscription.Name));
        Assert.Equal(HealthStatus.Healthy, (await harness.CheckHealthAsync()).Status);

        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Stopped, harness.State(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task A_failed_batch_is_retried_until_it_succeeds()
    {
        var containers = await fixture.CreateContainersAsync();
        await using var harness = CreateHarness(containers);
        await WriteAsync(containers.Monitored, NewStream(), 0, 2);
        harness.Settings.Failing[FirstCatchupSubscription.Name] = true;

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.State(FirstCatchupSubscription.Name) == CosmosDBCatchupSubscriptionState.Retrying);

        var degraded = await harness.CheckHealthAsync();
        Assert.Equal(HealthStatus.Degraded, degraded.Status);
        Assert.Contains(FirstCatchupSubscription.Name, degraded.Description);
        Assert.IsType<InvalidOperationException>(degraded.Exception);
        Assert.Empty(harness.Settings.Received(FirstCatchupSubscription.Name));

        harness.Settings.Failing[FirstCatchupSubscription.Name] = false;
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await WaitUntilAsync(() => harness.State(FirstCatchupSubscription.Name) == CosmosDBCatchupSubscriptionState.Running);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1"], harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal(HealthStatus.Healthy, (await harness.CheckHealthAsync()).Status);
    }

    [Fact]
    public async Task Resumes_from_the_lease_container_after_a_restart()
    {
        var containers = await fixture.CreateContainersAsync();
        var stream = NewStream();
        await using var harness = CreateHarness(containers);
        await WriteAsync(containers.Monitored, stream, 0, 2);

        var first = new FirstCatchupSubscription(harness.Services);
        await first.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await first.StopAsync(TestContext.Current.CancellationToken);

        await WriteAsync(containers.Monitored, stream, 2, 2);

        await using var restarted = CreateHarness(containers);
        var second = new FirstCatchupSubscription(restarted.Services);
        await second.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => restarted.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await second.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-2", "event-3"], restarted.Settings.Received(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task A_disabled_subscription_does_not_read_the_change_feed()
    {
        var containers = await fixture.CreateContainersAsync();
        await using var harness = CreateHarness(containers, enabled: false);
        await WriteAsync(containers.Monitored, NewStream(), 0, 2);

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(1000, TestContext.Current.CancellationToken);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Empty(harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.False(harness.Registry.TryGetStatus(FirstCatchupSubscription.Name, out _));
        Assert.Contains(harness.Logs.Messages, m => m == $"Subscription {FirstCatchupSubscription.Name} is disabled.");
    }

    [Fact]
    public async Task A_failed_start_reports_stopped_with_the_error_and_is_unhealthy()
    {
        var containers = await fixture.CreateContainersAsync();
        await using var harness = CreateHarness((containers.Monitored, $"missing-{Guid.NewGuid():N}"));

        var subscription = new FirstCatchupSubscription(harness.Services);
        var error = await Assert.ThrowsAnyAsync<Exception>(() => subscription.StartAsync(TestContext.Current.CancellationToken));

        Assert.Equal(CosmosDBCatchupSubscriptionState.Stopped, harness.State(FirstCatchupSubscription.Name));
        var health = await harness.CheckHealthAsync();
        Assert.Equal(HealthStatus.Degraded, health.Status);
        Assert.Same(error, health.Exception);

        await subscription.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Starting_from_a_date_time_skips_earlier_changes()
    {
        var containers = await fixture.CreateContainersAsync();
        var stream = NewStream();
        await WriteAsync(containers.Monitored, stream, 0, 2);
        // _ts has one second resolution, so the start time is kept clear of both writes.
        await Task.Delay(1500, TestContext.Current.CancellationToken);
        var startTime = DateTime.UtcNow;
        await Task.Delay(1500, TestContext.Current.CancellationToken);
        await WriteAsync(containers.Monitored, stream, 2, 2);

        await using var harness = CreateHarness(containers, configure: options =>
        {
            options.StartingPositionType = StartingPositionType.FromDateTime;
            options.StartingPositionDateTime = startTime;
        });
        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count >= 2);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-2", "event-3"], harness.Settings.Received(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task Starting_from_latest_skips_existing_changes()
    {
        var containers = await fixture.CreateContainersAsync();
        var stream = NewStream();
        await WriteAsync(containers.Monitored, stream, 0, 2);

        await using var harness = CreateHarness(containers, configure: options => options.StartingPositionType = StartingPositionType.Latest);
        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        // The processor initializes its leases in the background; changes written before that would also be skipped.
        await Task.Delay(3000, TestContext.Current.CancellationToken);
        await WriteAsync(containers.Monitored, stream, 2, 2);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count >= 2);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-2", "event-3"], harness.Settings.Received(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task Starting_twice_replaces_the_processor_without_duplicate_delivery()
    {
        var containers = await fixture.CreateContainersAsync();
        var stream = NewStream();
        await using var harness = CreateHarness(containers);
        await WriteAsync(containers.Monitored, stream, 0, 2);

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WriteAsync(containers.Monitored, stream, 2, 2);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count >= 4);
        await Task.Delay(500, TestContext.Current.CancellationToken);

        Assert.Equal(CosmosDBCatchupSubscriptionState.Running, harness.State(FirstCatchupSubscription.Name));
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1", "event-2", "event-3"], harness.Settings.Received(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task Events_from_one_append_are_delivered_in_sub_sequence_order()
    {
        var containers = await fixture.CreateContainersAsync();
        var stream = NewStream();
        await using var harness = CreateHarness(containers);
        var batch = fixture.Client.GetContainer(CosmosDBFixture.DatabaseName, containers.Monitored).CreateTransactionalBatch(new PartitionKey(stream));
        for (var i = 0; i < 3; i++)
        {
            batch.CreateItem(new Dictionary<string, object>
            {
                ["id"] = Guid.NewGuid().ToString(),
                ["type"] = "evt",
                ["streamName"] = stream,
                ["ver"] = i + 1,
                ["dataType"] = EventType,
                ["data"] = $"event-{i}",
                ["subSeq"] = i,
            });
        }
        using (var response = await batch.ExecuteAsync(TestContext.Current.CancellationToken))
        {
            Assert.True(response.IsSuccessStatusCode);
        }

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 3);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1", "event-2"], harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal([0UL, 1UL, 2UL], harness.Settings.Events.Select(e => e.Cursor.SubSequenceNumber));
        Assert.Equal([1UL, 2UL, 3UL], harness.Settings.Events.Select(e => e.StreamVersion));
    }

    private static string NewStream() => $"test/{Guid.NewGuid():N}";

    private Harness CreateHarness((string Monitored, string Leases) containers, bool enabled = true, Action<CosmosDBCatchupSubscriptionOptions>? configure = null)
    {
        var typeMapper = new TestTypeMapper();
        typeMapper.AddType(typeof(string), EventType);

        var logs = new CapturingLoggerProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddProvider(logs));
        services.AddSingleton(logs);
        services.AddSingleton(new TestSubscriptionSettings());
        services.AddSingleton<CosmosDBCatchupSubscriptionStatusRegistry>();
        services.AddSingleton(sp => new CosmosDBCatchupSubscriptionMetrics(sp.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>()));
        services.AddKeyedSingleton(ConnectionName, fixture.Client);
        services.AddOptions<CosmosDBCatchupSubscriptionOptions>(FirstCatchupSubscription.Name).Configure(options =>
        {
            options.Enabled = enabled;
            options.ConnectionName = ConnectionName;
            options.DatabaseName = CosmosDBFixture.DatabaseName;
            options.MonitoredContainerName = containers.Monitored;
            options.LeaseContainerName = containers.Leases;
            options.InstanceName = "test-instance";
            options.PollingIntervalInMilliseconds = 100;
            options.GrainStorageSerializer = new JsonStringSerializer();
            options.TypeMapper = typeMapper;
            options.StreamNameParser = new DefaultCosmosDBStreamNameParser();
            configure?.Invoke(options);
        });
        return new Harness(services.BuildServiceProvider());
    }

    private async Task WriteAsync(string container, string stream, int start, int count)
    {
        var target = fixture.Client.GetContainer(CosmosDBFixture.DatabaseName, container);
        for (var i = start; i < start + count; i++)
        {
            var item = new Dictionary<string, object>
            {
                ["id"] = Guid.NewGuid().ToString(),
                ["type"] = "evt",
                ["streamName"] = stream,
                ["ver"] = i + 1,
                ["dataType"] = EventType,
                ["data"] = $"event-{i}",
                ["subSeq"] = 0,
            };
            await target.CreateItemAsync(item, new PartitionKey(stream), cancellationToken: TestContext.Current.CancellationToken);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(Timeout);
        while (!condition())
        {
            await Task.Delay(100, timeout.Token);
        }
    }

    private sealed class Harness(ServiceProvider services) : IAsyncDisposable
    {
        public IServiceProvider Services => services;
        public TestSubscriptionSettings Settings => services.GetRequiredService<TestSubscriptionSettings>();
        public CosmosDBCatchupSubscriptionStatusRegistry Registry => services.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>();
        public CapturingLoggerProvider Logs => services.GetRequiredService<CapturingLoggerProvider>();

        public CosmosDBCatchupSubscriptionState? State(string name) => Registry.TryGetStatus(name, out var status) ? status!.State : null;

        public Task<HealthCheckResult> CheckHealthAsync()
        {
            var healthCheck = new CosmosDBCatchupSubscriptionsHealthCheck(Registry);
            var context = new HealthCheckContext
            {
                Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Degraded, null)
            };
            return healthCheck.CheckHealthAsync(context);
        }

        public ValueTask DisposeAsync() => services.DisposeAsync();
    }

    public sealed class TestSubscriptionSettings
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _received = new();

        public ConcurrentDictionary<string, bool> Failing { get; } = new();

        public List<string> Received(string subscriptionName) => _received.GetOrAdd(subscriptionName, _ => new()).ToList();

        public ConcurrentQueue<IStreamedEvent<object>> Events { get; } = new();

        public void Record(string subscriptionName, IEnumerable<IStreamedEvent<object>> events)
        {
            var received = _received.GetOrAdd(subscriptionName, _ => new());
            foreach (var streamedEvent in events)
            {
                Events.Enqueue(streamedEvent);
                received.Enqueue((string)streamedEvent.Event);
            }
        }
    }

    public sealed class FirstCatchupSubscription(IServiceProvider serviceProvider)
        : CosmosDBCatchupSubscription<FirstCatchupSubscription, CosmosDBCatchupSubscriptionOptions>(serviceProvider)
    {
        private readonly TestSubscriptionSettings _settings = serviceProvider.GetRequiredService<TestSubscriptionSettings>();

        protected override Task HandleStreamEventsAsync(ChangeFeedProcessorContext context, List<IStreamedEvent<object>> streamEvents, CancellationToken cancellationToken)
        {
            if (_settings.Failing.GetValueOrDefault(Name))
            {
                throw new InvalidOperationException("The handler rejected the batch.");
            }
            _settings.Record(Name, streamEvents);
            return Task.CompletedTask;
        }
    }

    public sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> _messages = new();

        public IReadOnlyCollection<string> Messages => _messages.ToArray();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(_messages);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                => messages.Enqueue(formatter(state, exception));
        }
    }

    private sealed class TestTypeMapper : TypeMapper;

    private sealed class JsonStringSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)(object)JsonSerializer.Deserialize<string>(input.ToString())!;
    }
}
