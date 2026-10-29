using System.Threading.Channels;

using Continuum.Orleans.Events;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Polly;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     Reads streamed events from a channel, hands them to a batch handler, and records the processed position with a
///     checkpoint monitor.
/// </summary>
internal sealed class KurrentDBCatchupBatchReceiver
{
    private readonly ChannelReader<StreamedEvent<object>> _reader;
    private readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    private readonly Func<List<IStreamedEvent<object>>, Task> _onNextBatchAsync;
    private readonly int _maxBatchSize;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _incompleteTransactionTimeout;

    /// <summary>
    ///     The default time to wait for the rest of an open transaction before handing it off incomplete.
    /// </summary>
    public static readonly TimeSpan DefaultIncompleteTransactionTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCatchupBatchReceiver" /> class.
    /// </summary>
    /// <param name="reader">The channel the subscription writes events and checkpoint markers to.</param>
    /// <param name="checkpointMonitor">The monitor processed positions are recorded with.</param>
    /// <param name="onNextBatchAsync">The handler each batch is passed to.</param>
    /// <param name="maxBatchSize">The number of events at which a batch is considered full.</param>
    /// <param name="resiliencePipeline">The pipeline each handler call runs through, or <see langword="null" /> for none.</param>
    /// <param name="logger">The logger recovery from failed batches is reported to.</param>
    /// <param name="timeProvider">The time provider used to measure how long a batch has been failing and how long a transaction has been waiting.</param>
    /// <param name="incompleteTransactionTimeout">How long to wait for the rest of an open transaction before handing it off incomplete, or <see langword="null" /> for <see cref="DefaultIncompleteTransactionTimeout" />.</param>
    public KurrentDBCatchupBatchReceiver(ChannelReader<StreamedEvent<object>> reader,
        KurrentDBCheckpointMonitor checkpointMonitor,
        Func<List<IStreamedEvent<object>>, Task> onNextBatchAsync,
        int maxBatchSize,
        ResiliencePipeline? resiliencePipeline = null,
        ILogger? logger = null,
        TimeProvider? timeProvider = null,
        TimeSpan? incompleteTransactionTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(checkpointMonitor);
        ArgumentNullException.ThrowIfNull(onNextBatchAsync);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxBatchSize, 1);

        _reader = reader;
        _checkpointMonitor = checkpointMonitor;
        _onNextBatchAsync = onNextBatchAsync;
        _maxBatchSize = maxBatchSize;
        _resiliencePipeline = resiliencePipeline ?? ResiliencePipeline.Empty;
        _logger = logger ?? NullLogger.Instance;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _incompleteTransactionTimeout = incompleteTransactionTimeout ?? DefaultIncompleteTransactionTimeout;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(_incompleteTransactionTimeout, TimeSpan.Zero, nameof(incompleteTransactionTimeout));
    }

    /// <summary>
    ///     Processes events until the channel is completed or <paramref name="cancellationToken" /> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting for events and retrying.</param>
    /// <returns>A task that completes when the channel is completed and drained.</returns>
    /// <remarks>
    ///     A batch is never handed off while it ends part way through a transaction, even when that means exceeding the
    ///     maximum batch size or waiting for more events. Completion is tracked per stream, because a consumer only ever
    ///     receives a stream's partition of a transaction. Events of a transaction are contiguous in <c>$all</c>, so an open
    ///     transaction is handed off incomplete, with a warning, when an event from outside it arrives or when nothing
    ///     arrives within the incomplete transaction timeout; its remaining events were filtered out. A failed batch
    ///     gives up a <see cref="KurrentDBBatchProcessingException" /> escapes without recording the position, so no
    ///     event is skipped.
    /// </remarks>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        ulong lastStreamPosition = 0;
        var eventBatch = new List<IStreamedEvent<object>>();
        // Streams whose current transaction has started but not yet delivered its last event.
        var openTransactions = new Dictionary<string, OpenTransaction>(StringComparer.Ordinal);
        while (true)
        {
            var timedOut = false;
            if (openTransactions.Count == 0)
            {
                if (!await _reader.WaitToReadAsync(cancellationToken))
                {
                    break;
                }
            }
            else
            {
                using var timeout = new CancellationTokenSource(_incompleteTransactionTimeout, _timeProvider);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
                try
                {
                    if (!await _reader.WaitToReadAsync(linked.Token))
                    {
                        break;
                    }
                }
                catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    timedOut = true;
                }
            }

            if (timedOut)
            {
                // A transaction is committed in one go, so its events sit together in $all. Nothing arriving for this
                // long means the rest of it was filtered out and will never come.
                AbandonOpenTransactions(openTransactions, $"no event arrived within {_incompleteTransactionTimeout}");
            }

            while (_reader.TryRead(out var streamedEvent))
            {
                lastStreamPosition = streamedEvent.Cursor.ToKurrentDBPosition(streamedEvent.StreamName.Value);

                if (streamedEvent.Event is CheckpointMarker)
                {
                    continue;
                }

                if (openTransactions.Count > 0 && !IsPartOfOpenTransaction(openTransactions, streamedEvent))
                {
                    // Events of a transaction are contiguous in $all, so an event from outside it means the rest of the
                    // open transaction was filtered out and will never come.
                    AbandonOpenTransactions(openTransactions, "an event from outside the transaction arrived");
                }

                eventBatch.Add(streamedEvent);
                TrackTransaction(openTransactions, streamedEvent);

                if (eventBatch.Count >= _maxBatchSize && openTransactions.Count == 0)
                {
                    break;
                }
            }

            if (openTransactions.Count > 0)
            {
                continue;
            }

            if (eventBatch.Count > 0)
            {
                await ProcessBatchAsync(eventBatch, lastStreamPosition, cancellationToken);
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition, eventBatch.Count);
                eventBatch.Clear();
            }
            else if (lastStreamPosition > 0)
            {
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition);
            }
        }
    }

    private async Task ProcessBatchAsync(List<IStreamedEvent<object>> eventBatch, ulong lastStreamPosition, CancellationToken cancellationToken)
    {
        var first = eventBatch[0];
        var state = new KurrentDBBatchRetryState(eventBatch.Count, first.Cursor.ToKurrentDBPosition(first.StreamName.Value),
            lastStreamPosition, _timeProvider);
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(KurrentDBBatchRetryState.Key, state);
        try
        {
            await _resiliencePipeline.ExecuteAsync(static async (_, args) =>
            {
                try
                {
                    await args.Handler(args.Batch);
                }
                catch (Exception) when (!args.CancellationToken.IsCancellationRequested)
                {
                    args.State.RecordFailure();
                    throw;
                }
            }, context, (Handler: _onNextBatchAsync, Batch: eventBatch, State: state, CancellationToken: cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            throw new KurrentDBBatchProcessingException(state, ex);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }

        if (state.FailedAttempts > 0)
        {
            _logger.BatchRecovered(state.EventCount, state.FirstPosition, state.LastPosition, state.FailedAttempts, state.Elapsed);
        }
    }

    private static bool IsPartOfOpenTransaction(Dictionary<string, OpenTransaction> openTransactions, StreamedEvent<object> streamedEvent)
    {
        if (streamedEvent.Metadata.TransactionId is not { } transactionId)
        {
            return false;
        }
        foreach (var open in openTransactions.Values)
        {
            if (open.TransactionId == transactionId)
            {
                return true;
            }
        }
        return false;
    }

    private void AbandonOpenTransactions(Dictionary<string, OpenTransaction> openTransactions, string reason)
    {
        foreach (var (streamName, open) in openTransactions)
        {
            _logger.HandingOffIncompleteTransaction(open.TransactionId, streamName, open.ReceivedCount, open.PartitionSize, reason);
        }
        openTransactions.Clear();
    }

    private static void TrackTransaction(Dictionary<string, OpenTransaction> openTransactions, StreamedEvent<object> streamedEvent)
    {
        var metadata = streamedEvent.Metadata;
        var streamName = streamedEvent.StreamName.Value;
        bool isOpen = metadata.TransactionId is not null &&
            metadata.TransactionPartitionSize is { } partitionSize &&
            metadata.TransactionPartitionIndex is { } partitionIndex &&
            partitionIndex < partitionSize - 1;
        if (isOpen)
        {
            var receivedCount = openTransactions.TryGetValue(streamName, out var existing) && existing.TransactionId == metadata.TransactionId!.Value
                ? existing.ReceivedCount + 1
                : 1;
            openTransactions[streamName] = new OpenTransaction(metadata.TransactionId!.Value, metadata.TransactionPartitionSize!.Value, receivedCount);
        }
        else
        {
            openTransactions.Remove(streamName);
        }
    }

    private readonly record struct OpenTransaction(Guid TransactionId, int PartitionSize, int ReceivedCount);
}

internal static partial class KurrentDBCatchupBatchReceiverLog
{
	[LoggerMessage(Level = LogLevel.Information, Message = "Recovered processing a batch of {EventCount} events from position {FirstPosition} to {LastPosition} after {FailedAttempts} failed attempts over {Elapsed}.")]
	public static partial void BatchRecovered(this ILogger logger, int eventCount, ulong firstPosition, ulong lastPosition, int failedAttempts, TimeSpan elapsed);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Handing off incomplete transaction {TransactionId} in stream {StreamName} with {ReceivedCount} of {PartitionSize} events because {Reason}; the rest were most likely filtered out.")]
	public static partial void HandingOffIncompleteTransaction(this ILogger logger, Guid transactionId, string streamName, int receivedCount, int partitionSize, string reason);
}
