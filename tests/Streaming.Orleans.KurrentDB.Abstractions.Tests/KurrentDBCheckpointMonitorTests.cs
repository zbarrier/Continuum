using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using Microsoft.Extensions.Logging.Abstractions;

namespace Continuum.Streaming.Orleans.KurrentDB.Abstractions.Tests;

/// <summary>
/// Verifies that the checkpoint monitor commits after enough events or enough time, never moves the checkpoint
/// backwards, treats the store's default position as no checkpoint, and flushes pending progress when closed.
/// </summary>
public class KurrentDBCheckpointMonitorTests
{
    private const string SubscriptionName = "orders";

    private sealed class ManualTimeProvider : TimeProvider
    {
        private long _ticks;

        public override long TimestampFrequency => TimeSpan.TicksPerSecond;

        public override long GetTimestamp() => _ticks;

        public void Advance(TimeSpan by) => _ticks += by.Ticks;
    }

    private sealed class FakeCheckpointStore(ulong storedPosition = 0) : ICheckpointStore<ulong>
    {
        public int Reads { get; private set; }

        public List<ulong> Stored { get; } = [];

        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default)
        {
            Reads++;
            return Task.FromResult(storedPosition);
        }

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default)
        {
            Stored.Add(streamPosition);
            return Task.CompletedTask;
        }
    }

    private static KurrentDBCheckpointMonitor CreateMonitor(FakeCheckpointStore store, TimeProvider? timeProvider = null, ulong maxEventsBeforeCommit = 3, int maxSecondsBeforeCommit = 20)
    {
        var options = new KurrentDBCheckpointMonitorOptions
        {
            MaxEventsBeforeCommit = maxEventsBeforeCommit,
            MaxTimeBeforeCommit = TimeSpan.FromSeconds(maxSecondsBeforeCommit),
        };
        return new KurrentDBCheckpointMonitor(NullLogger.Instance, options, store, SubscriptionName, KurrentDBStreams.All, timeProvider ?? new ManualTimeProvider());
    }

    [Fact]
    public async Task Returns_null_when_the_store_has_no_checkpoint()
    {
        var monitor = CreateMonitor(new FakeCheckpointStore());

        Assert.Null(await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Returns_the_stored_checkpoint_and_caches_it()
    {
        var store = new FakeCheckpointStore(42);
        var monitor = CreateMonitor(store);

        Assert.Equal(42UL, await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken));
        Assert.Equal(42UL, await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken));
        Assert.Equal(1, store.Reads);
    }

    [Fact]
    public async Task Does_not_commit_before_the_event_or_time_limit()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store);

        Assert.False(await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken));
        Assert.False(await monitor.TryCheckpointAsync(2, TestContext.Current.CancellationToken));
        Assert.Empty(store.Stored);
    }

    [Fact]
    public async Task Commits_the_latest_position_when_the_event_limit_is_reached()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store);

        await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken);
        await monitor.TryCheckpointAsync(2, TestContext.Current.CancellationToken);

        Assert.True(await monitor.TryCheckpointAsync(3, TestContext.Current.CancellationToken));
        Assert.Equal([3UL], store.Stored);
    }

    [Fact]
    public async Task Counts_every_event_in_a_batch_towards_the_event_limit()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store);

        Assert.True(await monitor.TryCheckpointAsync(4, 4, TestContext.Current.CancellationToken));
        Assert.Equal([4UL], store.Stored);
    }

    [Fact]
    public async Task Restarts_the_event_count_after_a_commit()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store);

        for (ulong position = 1; position <= 5; position++)
        {
            await monitor.TryCheckpointAsync(position, TestContext.Current.CancellationToken);
        }

        Assert.Equal([3UL], store.Stored);
        Assert.True(await monitor.TryCheckpointAsync(6, TestContext.Current.CancellationToken));
        Assert.Equal([3UL, 6UL], store.Stored);
    }

    [Fact]
    public async Task Commits_when_the_time_limit_has_passed()
    {
        var store = new FakeCheckpointStore();
        var clock = new ManualTimeProvider();
        var monitor = CreateMonitor(store, clock, maxEventsBeforeCommit: 1_000);

        Assert.False(await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken));
        clock.Advance(TimeSpan.FromSeconds(20));

        Assert.True(await monitor.TryCheckpointAsync(2, TestContext.Current.CancellationToken));
        Assert.Equal([2UL], store.Stored);
    }

    [Fact]
    public async Task Restarts_the_time_limit_after_a_commit()
    {
        var store = new FakeCheckpointStore();
        var clock = new ManualTimeProvider();
        var monitor = CreateMonitor(store, clock, maxEventsBeforeCommit: 1_000);

        clock.Advance(TimeSpan.FromSeconds(20));
        Assert.True(await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken));

        clock.Advance(TimeSpan.FromSeconds(19));
        Assert.False(await monitor.TryCheckpointAsync(2, TestContext.Current.CancellationToken));
        Assert.Equal([1UL], store.Stored);
    }

    [Theory]
    [InlineData(5UL)]
    [InlineData(4UL)]
    public async Task Ignores_positions_at_or_before_the_current_position(ulong position)
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store, maxEventsBeforeCommit: 1);

        await monitor.TryCheckpointAsync(5, TestContext.Current.CancellationToken);

        Assert.False(await monitor.TryCheckpointAsync(position, TestContext.Current.CancellationToken));
        Assert.Equal([5UL], store.Stored);
    }

    [Fact]
    public async Task Ignores_positions_at_or_before_the_stored_checkpoint()
    {
        var store = new FakeCheckpointStore(10);
        var monitor = CreateMonitor(store, maxEventsBeforeCommit: 1);
        await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken);

        Assert.False(await monitor.TryCheckpointAsync(10, TestContext.Current.CancellationToken));
        Assert.True(await monitor.TryCheckpointAsync(11, TestContext.Current.CancellationToken));
        Assert.Equal([11UL], store.Stored);
    }

    [Fact]
    public async Task Close_commits_a_pending_position()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store);
        await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken);

        await monitor.CloseAsync(TestContext.Current.CancellationToken);

        Assert.Equal([1UL], store.Stored);
    }

    [Fact]
    public async Task Close_does_not_commit_when_nothing_is_pending()
    {
        var store = new FakeCheckpointStore();
        var monitor = CreateMonitor(store, maxEventsBeforeCommit: 1);
        await monitor.TryCheckpointAsync(1, TestContext.Current.CancellationToken);

        await monitor.CloseAsync(TestContext.Current.CancellationToken);

        Assert.Equal([1UL], store.Stored);
    }

    [Fact]
    public async Task Close_forgets_the_current_position()
    {
        var store = new FakeCheckpointStore(7);
        var monitor = CreateMonitor(store);
        await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken);

        await monitor.CloseAsync(TestContext.Current.CancellationToken);
        await monitor.GetLastCheckpointAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, store.Reads);
    }

    [Fact]
    public void Rejects_missing_dependencies()
    {
        var options = new KurrentDBCheckpointMonitorOptions();
        var store = new FakeCheckpointStore();

        Assert.Throws<ArgumentNullException>(() => new KurrentDBCheckpointMonitor(null, options, store, SubscriptionName, KurrentDBStreams.All));
        Assert.Throws<ArgumentNullException>(() => new KurrentDBCheckpointMonitor(NullLogger.Instance, null, store, SubscriptionName, KurrentDBStreams.All));
        Assert.Throws<ArgumentNullException>(() => new KurrentDBCheckpointMonitor(NullLogger.Instance, options, null, SubscriptionName, KurrentDBStreams.All));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_a_missing_subscription_or_stream_name(string? name)
    {
        var options = new KurrentDBCheckpointMonitorOptions();
        var store = new FakeCheckpointStore();

        Assert.ThrowsAny<ArgumentException>(() => new KurrentDBCheckpointMonitor(NullLogger.Instance, options, store, name, KurrentDBStreams.All));
        Assert.ThrowsAny<ArgumentException>(() => new KurrentDBCheckpointMonitor(NullLogger.Instance, options, store, SubscriptionName, name));
    }
}
