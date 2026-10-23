using System.Collections.Concurrent;
using System.Text;

using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.Streams;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests.Integration;

/// <summary>
/// Runs catch-up subscriptions against a real KurrentDB to verify checkpointing while running and on shutdown, resuming
/// from the stored checkpoint, retrying transient failures, isolation of a terminal failure to its own subscription, and
/// redelivery of the failed batch after a restart.
/// </summary>
[Collection(KurrentDBCollection.Name)]
public class KurrentDBCatchupSubscriptionIntegrationTests(KurrentDBFixture fixture)
{
    private const string ConnectionName = "kurrentdb";
    private const string EventType = "test-event";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task Flushes_the_processed_position_on_shutdown()
    {
        await using var harness = CreateHarness();
        var stream = await AppendAsync(3);
        harness.Settings.Streams[FirstCatchupSubscription.Name] = stream;

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 3);

        // The default cadence commits after 10,000 events or 20 seconds, so nothing is stored yet.
        Assert.False(harness.CheckpointStore.Positions.ContainsKey(FirstCatchupSubscription.Name));

        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2UL, harness.CheckpointStore.Positions[FirstCatchupSubscription.Name]);
        Assert.True(harness.Registry.TryGetStatus(FirstCatchupSubscription.Name, out var status));
        Assert.Equal(KurrentDBCatchupSubscriptionState.Stopped, status!.State);
    }

    [Fact]
    public async Task A_terminal_failure_stops_only_its_subscription_and_is_redelivered_after_restart()
    {
        await using var harness = CreateHarness();
        harness.Settings.Streams[FirstCatchupSubscription.Name] = await AppendAsync(2);
        harness.Settings.Streams[SecondCatchupSubscription.Name] = await AppendAsync(2);
        harness.Settings.Failing.Add(FirstCatchupSubscription.Name);

        var failing = new FirstCatchupSubscription(harness.Services);
        var healthy = new SecondCatchupSubscription(harness.Services);
        await failing.StartAsync(TestContext.Current.CancellationToken);
        await healthy.StartAsync(TestContext.Current.CancellationToken);

        await WaitUntilAsync(() => harness.Registry.TryGetStatus(FirstCatchupSubscription.Name, out var s) && s!.State == KurrentDBCatchupSubscriptionState.Faulted);
        await WaitUntilAsync(() => harness.Settings.Received(SecondCatchupSubscription.Name).Count == 2);

        Assert.True(harness.Registry.TryGetStatus(SecondCatchupSubscription.Name, out var healthyStatus));
        Assert.Equal(KurrentDBCatchupSubscriptionState.Running, healthyStatus!.State);

        var health = await harness.CheckHealthAsync();
        Assert.Equal(HealthStatus.Unhealthy, health.Status);
        Assert.Contains(FirstCatchupSubscription.Name, health.Description);
        Assert.IsType<InvalidOperationException>(health.Exception);

        await failing.StopAsync(TestContext.Current.CancellationToken);
        await healthy.StopAsync(TestContext.Current.CancellationToken);

        Assert.False(harness.CheckpointStore.Positions.ContainsKey(FirstCatchupSubscription.Name));
        Assert.Equal(1UL, harness.CheckpointStore.Positions[SecondCatchupSubscription.Name]);

        // A restarted worker gets a fresh registry, and the fixed handler receives the failed batch again.
        await using var restarted = CreateHarness(harness.CheckpointStore);
        restarted.Settings.Streams[FirstCatchupSubscription.Name] = harness.Settings.Streams[FirstCatchupSubscription.Name];
        var recovered = new FirstCatchupSubscription(restarted.Services);
        await recovered.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => restarted.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await recovered.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1"], restarted.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal(1UL, restarted.CheckpointStore.Positions[FirstCatchupSubscription.Name]);
        Assert.Equal(HealthStatus.Healthy, (await restarted.CheckHealthAsync()).Status);
    }

    [Fact]
    public async Task A_disabled_subscription_does_not_read_its_stream()
    {
        await using var harness = CreateHarness(enabled: false);
        harness.Settings.Streams[FirstCatchupSubscription.Name] = await AppendAsync(2);

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Empty(harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.False(harness.Registry.TryGetStatus(FirstCatchupSubscription.Name, out _));
        Assert.Contains(harness.Logs.Messages, m => m == $"Subscription '{FirstCatchupSubscription.Name}' is disabled.");
    }

    [Fact]
    public async Task Waits_for_a_stream_that_does_not_exist_yet()
    {
        await using var harness = CreateHarness();
        var stream = $"test-{Guid.NewGuid():N}";
        harness.Settings.Streams[FirstCatchupSubscription.Name] = stream;

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Logs.Messages.Any(m => m.Contains("has caught up")));

        Assert.True(harness.Registry.TryGetStatus(FirstCatchupSubscription.Name, out var status));
        Assert.Equal(KurrentDBCatchupSubscriptionState.Running, status!.State);

        await AppendAsync(2, stream);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1"], harness.Settings.Received(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task A_transient_failure_is_retried_until_the_batch_succeeds()
    {
        await using var harness = CreateHarness(configure: options =>
        {
            options.RetryOptions.BaseDelay = TimeSpan.FromMilliseconds(50);
            options.RetryOptions.MaxDelay = TimeSpan.FromMilliseconds(200);
        });
        harness.Settings.Streams[FirstCatchupSubscription.Name] = await AppendAsync(2);
        harness.Settings.TransientFailing[FirstCatchupSubscription.Name] = true;

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.State(FirstCatchupSubscription.Name) == KurrentDBCatchupSubscriptionState.Retrying);

        var degraded = await harness.CheckHealthAsync();
        Assert.Equal(HealthStatus.Degraded, degraded.Status);
        Assert.Contains(FirstCatchupSubscription.Name, degraded.Description);
        Assert.Empty(harness.Settings.Received(FirstCatchupSubscription.Name));

        harness.Settings.TransientFailing[FirstCatchupSubscription.Name] = false;
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await WaitUntilAsync(() => harness.State(FirstCatchupSubscription.Name) == KurrentDBCatchupSubscriptionState.Running);

        Assert.Equal(HealthStatus.Healthy, (await harness.CheckHealthAsync()).Status);

        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-0", "event-1"], harness.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal(1UL, harness.CheckpointStore.Positions[FirstCatchupSubscription.Name]);
        Assert.Equal(KurrentDBCatchupSubscriptionState.Stopped, harness.State(FirstCatchupSubscription.Name));
    }

    [Fact]
    public async Task Commits_the_checkpoint_while_running_once_the_event_threshold_is_reached()
    {
        await using var harness = CreateHarness(configure: options => options.CheckpointMonitorOptions.MaxEventsBeforeCommit = 2);
        harness.Settings.Streams[FirstCatchupSubscription.Name] = await AppendAsync(4);

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 4);
        await WaitUntilAsync(() => harness.CheckpointStore.Positions.ContainsKey(FirstCatchupSubscription.Name));

        // Committed by the event threshold before shutdown, not by the flush on stop.
        Assert.Equal(KurrentDBCatchupSubscriptionState.Running, harness.State(FirstCatchupSubscription.Name));

        await subscription.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3UL, harness.CheckpointStore.Positions[FirstCatchupSubscription.Name]);
    }

    [Fact]
    public async Task Resumes_after_the_stored_checkpoint_on_restart()
    {
        await using var harness = CreateHarness();
        var stream = await AppendAsync(2);
        harness.Settings.Streams[FirstCatchupSubscription.Name] = stream;

        var first = new FirstCatchupSubscription(harness.Services);
        await first.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await first.StopAsync(TestContext.Current.CancellationToken);

        await AppendAsync(2, stream, start: 2, expectedRevision: 1);

        await using var restarted = CreateHarness(harness.CheckpointStore);
        restarted.Settings.Streams[FirstCatchupSubscription.Name] = stream;
        var second = new FirstCatchupSubscription(restarted.Services);
        await second.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => restarted.Settings.Received(FirstCatchupSubscription.Name).Count == 2);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await second.StopAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["event-2", "event-3"], restarted.Settings.Received(FirstCatchupSubscription.Name));
        Assert.Equal(3UL, restarted.CheckpointStore.Positions[FirstCatchupSubscription.Name]);
    }

    [Fact]
    public async Task A_category_subscription_receives_every_tenant_and_parses_tenant_qualified_stream_names()
    {
        await using var harness = CreateHarness();
        var category = $"order{Guid.NewGuid():N}";
        var key = Guid.NewGuid().ToString();
        var acme = await AppendAsync(1, $"{category}-acme|{key}");
        var contoso = await AppendAsync(1, $"{category}-contoso|{key}");
        var untenanted = await AppendAsync(1, $"{category}-{key}");
        await AppendAsync(1);
        harness.Settings.Streams[FirstCatchupSubscription.Name] = "$all";
        harness.Settings.Filters[FirstCatchupSubscription.Name] = StreamFilter.Prefix($"{category}-");

        var subscription = new FirstCatchupSubscription(harness.Services);
        await subscription.StartAsync(TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => harness.Settings.ReceivedStreams(FirstCatchupSubscription.Name).Count == 3);
        await Task.Delay(500, TestContext.Current.CancellationToken);
        await subscription.StopAsync(TestContext.Current.CancellationToken);

        var streams = harness.Settings.ReceivedStreams(FirstCatchupSubscription.Name).OrderBy(s => s.TenantId ?? string.Empty, StringComparer.Ordinal).ToList();
        Assert.Equal([untenanted, acme, contoso], streams.Select(s => s.Value));
        Assert.All(streams, s => Assert.Equal(category, s.Category));
        Assert.All(streams, s => Assert.Equal(key, s.Key));
        Assert.Equal([null, "acme", "contoso"], streams.Select(s => s.TenantId));
    }

    private Harness CreateHarness(InMemoryCheckpointStore? checkpointStore = null, bool enabled = true,
        Action<KurrentDBCatchupSubscriptionOptions>? configure = null)
    {
        checkpointStore ??= new InMemoryCheckpointStore();
        var typeMapper = new TestTypeMapper();
        typeMapper.AddType(typeof(string), EventType);

        var logs = new CapturingLoggerProvider();
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddProvider(logs));
        services.AddSingleton(logs);
        services.AddSingleton(new TestSubscriptionSettings());
        services.AddSingleton<KurrentDBCatchupSubscriptionStatusRegistry>();
        services.AddSingleton(sp => new KurrentDBCatchupSubscriptionMetrics(sp.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>()));
        services.AddKeyedSingleton(ConnectionName, (_, _) => new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString)));
        foreach (var name in new[] { FirstCatchupSubscription.Name, SecondCatchupSubscription.Name })
        {
            services.AddOptions<KurrentDBCatchupSubscriptionOptions>(name).Configure(options =>
            {
                if (!enabled)
                {
                    options.Enabled = false;
                }
                options.ConnectionName = ConnectionName;
                options.CheckpointStore = checkpointStore;
                options.GrainStorageSerializer = new Utf8StringSerializer();
                options.TypeMapper = typeMapper;
                options.StreamNameParser = new DefaultKurrentDBStreamNameParser();
                configure?.Invoke(options);
            });
        }
        return new Harness(services.BuildServiceProvider(), checkpointStore);
    }

    private async Task<string> AppendAsync(int count, string? stream = null, int start = 0, ulong? expectedRevision = null)
    {
        stream ??= $"test-{Guid.NewGuid():N}";
        await using var client = new KurrentDBClient(KurrentDBClientSettings.Create(fixture.ConnectionString));
        var events = Enumerable.Range(start, count)
            .Select(i => new EventData(Uuid.NewUuid(), EventType, Encoding.UTF8.GetBytes($"event-{i}")))
            .ToList();
        var state = expectedRevision is { } revision ? StreamState.StreamRevision(revision) : StreamState.NoStream;
        await client.AppendToStreamAsync(stream, state, events, cancellationToken: TestContext.Current.CancellationToken);
        return stream;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(Timeout);
        while (!condition())
        {
            await Task.Delay(50, timeout.Token);
        }
    }

    private sealed class Harness(ServiceProvider services, InMemoryCheckpointStore checkpointStore) : IAsyncDisposable
    {
        public IServiceProvider Services => services;
        public InMemoryCheckpointStore CheckpointStore => checkpointStore;
        public TestSubscriptionSettings Settings => services.GetRequiredService<TestSubscriptionSettings>();
        public KurrentDBCatchupSubscriptionStatusRegistry Registry => services.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>();
        public CapturingLoggerProvider Logs => services.GetRequiredService<CapturingLoggerProvider>();

        public KurrentDBCatchupSubscriptionState? State(string name) => Registry.TryGetStatus(name, out var status) ? status!.State : null;

        public Task<HealthCheckResult> CheckHealthAsync()
        {
            var healthCheck = new KurrentDBCatchupSubscriptionsHealthCheck(Registry);
            var context = new HealthCheckContext { Registration = new HealthCheckRegistration("test", healthCheck, HealthStatus.Unhealthy, null) };
            return healthCheck.CheckHealthAsync(context);
        }

        public ValueTask DisposeAsync() => services.DisposeAsync();
    }

    public sealed class TestSubscriptionSettings
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _received = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<IStreamName>> _receivedStreams = new();

        public ConcurrentDictionary<string, string> Streams { get; } = new();
        public ConcurrentDictionary<string, IEventFilter> Filters { get; } = new();

        public List<IStreamName> ReceivedStreams(string subscriptionName) => _receivedStreams.GetOrAdd(subscriptionName, _ => new()).ToList();
        public ConcurrentBag<string> Failing { get; } = [];
        public ConcurrentDictionary<string, bool> TransientFailing { get; } = new();

        public List<string> Received(string subscriptionName) => _received.GetOrAdd(subscriptionName, _ => new()).ToList();

        public void Record(string subscriptionName, IEnumerable<IStreamedEvent<object>> events)
        {
            var received = _received.GetOrAdd(subscriptionName, _ => new());
            var receivedStreams = _receivedStreams.GetOrAdd(subscriptionName, _ => new());
            foreach (var streamedEvent in events)
            {
                received.Enqueue((string)streamedEvent.Event);
                receivedStreams.Enqueue(streamedEvent.StreamName);
            }
        }
    }

    public abstract class TestCatchupSubscription<TSelf>(IServiceProvider serviceProvider)
        : KurrentDBCatchupSubscription<TSelf, KurrentDBCatchupSubscriptionOptions>(serviceProvider)
        where TSelf : TestCatchupSubscription<TSelf>
    {
        private readonly TestSubscriptionSettings _settings = serviceProvider.GetRequiredService<TestSubscriptionSettings>();

        public override string StreamName => _settings.Streams[Name];

        public override IEventFilter? FilterOptions => _settings.Filters.GetValueOrDefault(Name);

        protected override Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents)
        {
            if (_settings.Failing.Contains(Name))
            {
                throw new InvalidOperationException("The handler rejected the batch.");
            }
            if (_settings.TransientFailing.GetValueOrDefault(Name))
            {
                throw new TimeoutException("The handler timed out.");
            }
            _settings.Record(Name, streamEvents);
            return Task.CompletedTask;
        }
    }

    public sealed class FirstCatchupSubscription(IServiceProvider serviceProvider) : TestCatchupSubscription<FirstCatchupSubscription>(serviceProvider);

    public sealed class SecondCatchupSubscription(IServiceProvider serviceProvider) : TestCatchupSubscription<SecondCatchupSubscription>(serviceProvider);

    public sealed class InMemoryCheckpointStore : ICheckpointStore<ulong>
    {
        public ConcurrentDictionary<string, ulong> Positions { get; } = new();

        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
            => Task.FromResult(Positions.GetValueOrDefault(subscriptionName));

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default)
        {
            Positions[subscriptionName] = streamPosition;
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

    private sealed class Utf8StringSerializer : IGrainStorageSerializer
    {
        public BinaryData Serialize<T>(T? input) => throw new NotSupportedException();

        public T Deserialize<T>(BinaryData input) => (T)(object)input.ToString();
    }
}
