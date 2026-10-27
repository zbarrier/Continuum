using System.Threading.Channels;

using Continuum.Events;
using Continuum.Orleans.Events;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Polly;
using Polly.Retry;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Tests;

/// <summary>
/// Verifies that the batch receiver hands events to the handler in order, keeps checkpoint markers out of batches
/// while still recording their position, and records the processed position after each batch.
/// </summary>
public class KurrentDBCatchupBatchReceiverTests
{
    private sealed record OrderPlaced(int Number);

    private sealed class FakeCheckpointStore : ICheckpointStore<ulong>
    {
        public List<ulong> Stored { get; } = [];

        public Task<ulong> GetLastCheckpointAsync(string subscriptionName, string partitionId, CancellationToken cancellationToken = default) => Task.FromResult(0UL);

        public Task StoreCheckpointAsync(string subscriptionName, string partitionId, ulong streamPosition, CancellationToken cancellationToken = default)
        {
            Stored.Add(streamPosition);
            return Task.CompletedTask;
        }
    }

    private readonly Channel<StreamedEvent<object>> _channel = Channel.CreateUnbounded<StreamedEvent<object>>();
    private readonly FakeCheckpointStore _store = new();
    private readonly List<List<IStreamedEvent<object>>> _batches = [];

    private KurrentDBCatchupBatchReceiver CreateReceiver(Func<List<IStreamedEvent<object>>, Task>? onNextBatchAsync = null, ulong maxEventsBeforeCommit = 1,
        int maxBatchSize = 100, ResiliencePipeline? resiliencePipeline = null, TimeSpan? incompleteTransactionTimeout = null)
    {
        var monitor = new KurrentDBCheckpointMonitor(NullLogger.Instance,
            new KurrentDBCheckpointMonitorOptions { MaxEventsBeforeCommit = maxEventsBeforeCommit },
            _store, "orders", KurrentDBStreams.All);
        return new KurrentDBCatchupBatchReceiver(_channel.Reader, monitor, onNextBatchAsync ?? (batch =>
        {
            _batches.Add([.. batch]);
            return Task.CompletedTask;
        }), maxBatchSize, resiliencePipeline, incompleteTransactionTimeout: incompleteTransactionTimeout);
    }

    private static StreamedEvent<object> Event(string streamName, ulong position, int number, TransactionInfo? transaction = null)
    {
        var name = new DefaultKurrentDBStreamNameParser().Parse(streamName);
        return new StreamedEvent<object>(Guid.NewGuid(), nameof(OrderPlaced), name, 0,
            KurrentDBStreamedEventConversions.ToCursor("store", name, position), DateTime.UtcNow, new OrderPlaced(number),
            transaction is null ? null : EventMetadata.Create(null, null, null, transaction));
    }

    private static ResiliencePipeline Retry(int maxRetryAttempts) => new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<TimeoutException>(),
            MaxRetryAttempts = maxRetryAttempts,
            Delay = TimeSpan.Zero
        })
        .Build();

    private async Task RunToCompletionAsync(KurrentDBCatchupBatchReceiver receiver, params StreamedEvent<object>[] events)
    {
        foreach (var streamedEvent in events)
        {
            await _channel.Writer.WriteAsync(streamedEvent, TestContext.Current.CancellationToken);
        }
        _channel.Writer.Complete();
        await receiver.RunAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Delivers_buffered_events_in_order_and_records_the_last_position()
    {
        await RunToCompletionAsync(CreateReceiver(), Event("order-1", 10, 1), Event("order-2", 20, 2), Event("order-1", 30, 3));

        var batch = Assert.Single(_batches);
        Assert.Equal([1, 2, 3], batch.Select(e => ((OrderPlaced)e.Event).Number));
        Assert.Equal([30UL], _store.Stored);
    }

    [Fact]
    public async Task Keeps_checkpoint_markers_out_of_batches_but_records_their_position()
    {
        await RunToCompletionAsync(CreateReceiver(), Event("order-1", 10, 1), new Position(50, 50).ToAllStreamCheckpointEvent("store"));

        var batch = Assert.Single(_batches);
        Assert.Equal([1], batch.Select(e => ((OrderPlaced)e.Event).Number));
        Assert.Equal([50UL], _store.Stored);
    }

    [Fact]
    public async Task Records_the_position_of_a_checkpoint_marker_without_calling_the_handler()
    {
        await RunToCompletionAsync(CreateReceiver(), new Position(50, 50).ToAllStreamCheckpointEvent("store"));

        Assert.Empty(_batches);
        Assert.Equal([50UL], _store.Stored);
    }

    [Fact]
    public async Task Delivers_events_written_after_a_previous_batch_as_a_new_batch()
    {
        var receiver = CreateReceiver();
        var run = receiver.RunAsync(TestContext.Current.CancellationToken);

        await _channel.Writer.WriteAsync(Event("order-1", 10, 1), TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => _batches.Count == 1);
        await _channel.Writer.WriteAsync(Event("order-1", 20, 2), TestContext.Current.CancellationToken);
        _channel.Writer.Complete();
        await run;

        Assert.Equal(2, _batches.Count);
        Assert.Equal([10UL, 20UL], _store.Stored);
    }

    [Fact]
    public async Task Does_not_record_the_position_when_the_handler_fails()
    {
        var receiver = CreateReceiver(_ => throw new InvalidOperationException("Handler failed."));

        var ex = await Assert.ThrowsAsync<KurrentDBBatchProcessingException>(() => RunToCompletionAsync(receiver, Event("order-1", 10, 1)));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task Completes_when_the_channel_is_completed_with_nothing_buffered()
    {
        await RunToCompletionAsync(CreateReceiver());

        Assert.Empty(_batches);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task Splits_batches_at_the_maximum_size_between_events_without_transactions()
    {
        var receiver = CreateReceiver(maxBatchSize: 2);

        await RunToCompletionAsync(receiver, Event("order-1", 10, 1), Event("order-1", 20, 2), Event("order-1", 30, 3));

        Assert.Equal([[1, 2], [3]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
        Assert.Equal([20UL, 30UL], _store.Stored);
    }

    [Fact]
    public async Task Exceeds_the_maximum_size_to_keep_a_transaction_in_one_batch()
    {
        var id = Guid.NewGuid();
        var receiver = CreateReceiver(maxBatchSize: 2);

        await RunToCompletionAsync(receiver,
            Event("order-1", 10, 1, new(id, 3, 3, 0)),
            Event("order-1", 20, 2, new(id, 3, 3, 1)),
            Event("order-1", 30, 3, new(id, 3, 3, 2)),
            Event("order-1", 40, 4));

        Assert.Equal([[1, 2, 3], [4]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
    }

    [Fact]
    public async Task Waits_for_the_rest_of_a_transaction_before_handing_off_a_batch()
    {
        var id = Guid.NewGuid();
        var receiver = CreateReceiver();
        var run = receiver.RunAsync(TestContext.Current.CancellationToken);

        await _channel.Writer.WriteAsync(Event("order-1", 10, 1, new(id, 2, 2, 0)), TestContext.Current.CancellationToken);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.Empty(_batches);

        await _channel.Writer.WriteAsync(Event("order-1", 20, 2, new(id, 2, 2, 1)), TestContext.Current.CancellationToken);
        _channel.Writer.Complete();
        await run;

        Assert.Equal([[1, 2]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
        Assert.Equal([20UL], _store.Stored);
    }

    [Fact]
    public async Task Tracks_transaction_completion_per_stream()
    {
        var id = Guid.NewGuid();
        var receiver = CreateReceiver(maxBatchSize: 1);

        await RunToCompletionAsync(receiver,
            Event("order-1", 10, 1, new(id, 4, 2, 0)),
            Event("order-2", 20, 2, new(id, 4, 2, 0)),
            Event("order-1", 30, 3, new(id, 4, 2, 1)),
            Event("order-2", 40, 4, new(id, 4, 2, 1)),
            Event("order-1", 50, 5));

        Assert.Equal([[1, 2, 3, 4], [5]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
    }

    [Fact]
    public async Task Does_not_deliver_or_record_an_incomplete_transaction_when_the_channel_completes()
    {
        await RunToCompletionAsync(CreateReceiver(), Event("order-1", 10, 1, new(Guid.NewGuid(), 2, 2, 0)));

        Assert.Empty(_batches);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task An_event_from_outside_an_open_transaction_hands_it_off_incomplete()
    {
        var id = Guid.NewGuid();
        var receiver = CreateReceiver(maxBatchSize: 1);

        await RunToCompletionAsync(receiver,
            Event("order-1", 10, 1, new(id, 3, 3, 0)),
            Event("order-1", 20, 2, new(id, 3, 3, 1)),
            Event("order-2", 40, 3));

        Assert.Equal([[1, 2, 3]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
        Assert.Equal([40UL], _store.Stored);
    }

    [Fact]
    public async Task An_event_from_another_transaction_hands_off_the_open_one_and_waits_for_its_own()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var receiver = CreateReceiver(maxBatchSize: 1);

        await RunToCompletionAsync(receiver,
            Event("order-1", 10, 1, new(first, 2, 2, 0)),
            Event("order-2", 20, 2, new(second, 2, 2, 0)),
            Event("order-2", 30, 3, new(second, 2, 2, 1)),
            Event("order-1", 40, 4));

        Assert.Equal([[1, 2, 3], [4]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
    }

    [Fact]
    public async Task A_checkpoint_marker_does_not_end_an_open_transaction()
    {
        var id = Guid.NewGuid();
        var receiver = CreateReceiver(maxBatchSize: 1);

        await RunToCompletionAsync(receiver,
            Event("order-1", 10, 1, new(id, 2, 2, 0)),
            new Position(15, 15).ToAllStreamCheckpointEvent("store"),
            Event("order-1", 20, 2, new(id, 2, 2, 1)));

        Assert.Equal([[1, 2]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
        Assert.Equal([20UL], _store.Stored);
    }

    [Fact]
    public async Task Hands_off_an_incomplete_transaction_when_nothing_arrives_within_the_timeout()
    {
        var receiver = CreateReceiver(incompleteTransactionTimeout: TimeSpan.FromMilliseconds(200));
        var run = receiver.RunAsync(TestContext.Current.CancellationToken);

        await _channel.Writer.WriteAsync(Event("order-1", 10, 1, new(Guid.NewGuid(), 2, 2, 0)), TestContext.Current.CancellationToken);
        await WaitUntilAsync(() => _batches.Count == 1);
        _channel.Writer.Complete();
        await run;

        Assert.Equal([[1]], _batches.Select(b => b.Select(e => ((OrderPlaced)e.Event).Number).ToArray()));
        Assert.Equal([10UL], _store.Stored);
    }

    [Fact]
    public async Task Cancellation_while_waiting_for_a_transaction_does_not_hand_it_off()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var receiver = CreateReceiver(incompleteTransactionTimeout: TimeSpan.FromMinutes(5));
        await _channel.Writer.WriteAsync(Event("order-1", 10, 1, new(Guid.NewGuid(), 2, 2, 0)), TestContext.Current.CancellationToken);
        var run = receiver.RunAsync(cancellation.Token);

        await Task.Delay(100, TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Empty(_batches);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task Retries_a_failed_batch_and_records_the_position_once_it_succeeds()
    {
        var attempts = 0;
        var receiver = CreateReceiver(batch => ++attempts < 3 ? throw new TimeoutException() : Task.CompletedTask, resiliencePipeline: Retry(3));

        await RunToCompletionAsync(receiver, Event("order-1", 10, 1));

        Assert.Equal(3, attempts);
        Assert.Equal([10UL], _store.Stored);
    }

    [Fact]
    public async Task Stops_without_recording_the_position_when_retries_are_exhausted()
    {
        var attempts = 0;
        var receiver = CreateReceiver(_ => { attempts++; throw new TimeoutException(); }, resiliencePipeline: Retry(2));

        var ex = await Assert.ThrowsAsync<KurrentDBBatchProcessingException>(() => RunToCompletionAsync(receiver, Event("order-1", 10, 1)));

        Assert.IsType<TimeoutException>(ex.InnerException);
        Assert.Equal(3, attempts);
        Assert.Equal(3, ex.State.FailedAttempts);
        Assert.Equal(10UL, ex.State.FirstPosition);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task Does_not_retry_errors_the_pipeline_does_not_handle()
    {
        var attempts = 0;
        var receiver = CreateReceiver(_ => { attempts++; throw new InvalidOperationException(); }, resiliencePipeline: Retry(3));

        var ex = await Assert.ThrowsAsync<KurrentDBBatchProcessingException>(() => RunToCompletionAsync(receiver, Event("order-1", 10, 1)));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal(1, attempts);
        Assert.Equal(1, ex.State.FailedAttempts);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public async Task Exposes_the_batch_and_failure_count_to_the_pipeline()
    {
        var observed = new List<(int FailedAttempts, int EventCount, ulong First, ulong Last)>();
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutException>(),
                MaxRetryAttempts = 5,
                Delay = TimeSpan.Zero,
                OnRetry = args =>
                {
                    var state = args.Context.Properties.GetValue(KurrentDBBatchRetryState.Key, null!);
                    observed.Add((state.FailedAttempts, state.EventCount, state.FirstPosition, state.LastPosition));
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
        var attempts = 0;
        var receiver = CreateReceiver(_ => ++attempts < 3 ? throw new TimeoutException() : Task.CompletedTask, resiliencePipeline: pipeline);

        await RunToCompletionAsync(receiver, Event("order-1", 10, 1), Event("order-1", 20, 2));

        Assert.Equal([(1, 2, 10UL, 20UL), (2, 2, 10UL, 20UL)], observed);
        Assert.Equal([20UL], _store.Stored);
    }

    [Fact]
    public async Task Cancellation_while_retrying_stops_without_recording_the_position()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<TimeoutException>(),
                MaxRetryAttempts = int.MaxValue,
                Delay = TimeSpan.FromMinutes(1)
            })
            .Build();
        var attempts = 0;
        var receiver = CreateReceiver(_ => { Interlocked.Increment(ref attempts); throw new TimeoutException(); }, resiliencePipeline: pipeline);
        await _channel.Writer.WriteAsync(Event("order-1", 10, 1), TestContext.Current.CancellationToken);
        var run = receiver.RunAsync(cancellation.Token);

        await WaitUntilAsync(() => Volatile.Read(ref attempts) == 1);
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Empty(_store.Stored);
    }

    [Theory]
    [InlineData(1, LogLevel.Warning)]
    [InlineData(5, LogLevel.Warning)]
    [InlineData(6, LogLevel.Error)]
    [InlineData(10, LogLevel.Error)]
    [InlineData(11, null)]
    [InlineData(19, null)]
    [InlineData(20, LogLevel.Error)]
    [InlineData(1000, LogLevel.Error)]
    public void Logs_early_retries_every_time_and_later_retries_every_tenth_attempt(int failedAttempts, LogLevel? expected)
    {
        Assert.Equal(expected, KurrentDBBatchRetryState.GetRetryLogLevel(failedAttempts));
    }

    [Fact]
    public async Task Stops_waiting_when_cancelled()
    {
        using var cancellation = new CancellationTokenSource();
        var run = CreateReceiver().RunAsync(cancellation.Token);

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
    }

    [Fact]
    public void Rejects_a_batch_size_below_one()
    {
        var monitor = new KurrentDBCheckpointMonitor(NullLogger.Instance, new KurrentDBCheckpointMonitorOptions(), _store, "orders", KurrentDBStreams.All);

        Assert.Throws<ArgumentOutOfRangeException>(() => new KurrentDBCatchupBatchReceiver(_channel.Reader, monitor, _ => Task.CompletedTask, 0));
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            await Task.Delay(10, timeout.Token);
        }
    }
}
