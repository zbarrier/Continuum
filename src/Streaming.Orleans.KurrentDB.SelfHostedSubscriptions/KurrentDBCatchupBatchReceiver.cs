using System.Threading.Channels;

using Continuum.Orleans.Events;
using Continuum.Streaming.Orleans.KurrentDB.Configuration;
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
    private readonly PartialTransactionHandling _partialTransactionHandling;
    private readonly Func<Guid, ulong, CancellationToken, Task<bool>>? _hasMoreTransactionEventsAsync;
    private readonly TimeSpan _verificationDelay;
    private readonly bool _groupTransactions;

    /// <summary>
    ///     How long an open transaction waits for its next event before KurrentDB is asked whether more of it exists, and
    ///     how long a failed verification waits before it is tried again.
    /// </summary>
    public static readonly TimeSpan DefaultVerificationDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     How long a verification read may take.
    /// </summary>
    public static readonly TimeSpan VerificationTimeout = TimeSpan.FromSeconds(5);

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
    /// <param name="partialTransactionHandling">What to do with a transaction KurrentDB has confirmed was only partially received.</param>
    /// <param name="hasMoreTransactionEventsAsync">
    ///     Reads the source after a position to find out whether the next event belongs to a transaction, or
    ///     <see langword="null" /> when it cannot be read, in which case an open transaction only ends when another event arrives.
    /// </param>
    /// <param name="verificationDelay">How long to wait before verifying an open transaction, or <see langword="null" /> for <see cref="DefaultVerificationDelay" />.</param>
    /// <param name="groupTransactions">
    ///     Whether to keep the events of a transaction in one batch. Only <c>$all</c> keeps a transaction's events
    ///     together, so a single-stream subscription, which may hold only part of a transaction, passes <see langword="false" />.
    /// </param>
    public KurrentDBCatchupBatchReceiver(ChannelReader<StreamedEvent<object>> reader,
        KurrentDBCheckpointMonitor checkpointMonitor,
        Func<List<IStreamedEvent<object>>, Task> onNextBatchAsync,
        int maxBatchSize,
        ResiliencePipeline? resiliencePipeline = null,
        ILogger? logger = null,
        TimeProvider? timeProvider = null,
        PartialTransactionHandling partialTransactionHandling = PartialTransactionHandling.Halt,
        Func<Guid, ulong, CancellationToken, Task<bool>>? hasMoreTransactionEventsAsync = null,
        TimeSpan? verificationDelay = null,
        bool groupTransactions = true)
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
        _partialTransactionHandling = partialTransactionHandling;
        _hasMoreTransactionEventsAsync = hasMoreTransactionEventsAsync;
        _verificationDelay = verificationDelay ?? DefaultVerificationDelay;
        _groupTransactions = groupTransactions;
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(_verificationDelay, TimeSpan.Zero, nameof(verificationDelay));
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
    ///     transaction is confirmed partial when an event from outside it arrives, or when nothing arrives for a while and
    ///     a verification read finds no further event of it; it is then handled per the partial transaction handling. A
    ///     failed verification confirms nothing, so the transaction keeps waiting. A failed batch
    ///     gives up a <see cref="KurrentDBBatchProcessingException" /> escapes without recording the position, so no
    ///     event is skipped.
    /// </remarks>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        ulong lastStreamPosition = 0;
        ulong lastTransactionPosition = 0;
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
                using var timeout = new CancellationTokenSource(_verificationDelay, _timeProvider);
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
                // Quiet alone proves nothing, since a dropped connection is quiet too, so KurrentDB is asked whether
                // more of the transaction exists before it is treated as partial.
                if (!await IsConfirmedPartialAsync(openTransactions.Values.First().TransactionId, lastTransactionPosition, cancellationToken))
                {
                    continue;
                }
                HandlePartialTransactions(openTransactions, lastTransactionPosition, "KurrentDB has no further event of it");
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
                    // Events of a transaction are contiguous in $all, so an event from outside it confirms the rest of
                    // the open transaction was filtered out and will never come.
                    HandlePartialTransactions(openTransactions, lastTransactionPosition, "an event from outside the transaction arrived");
                }

                eventBatch.Add(streamedEvent);
                if (_groupTransactions)
                {
                    TrackTransaction(openTransactions, streamedEvent);
                    if (streamedEvent.Metadata.TransactionId is not null)
                    {
                        lastTransactionPosition = lastStreamPosition;
                    }
                }

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

    private async Task<bool> IsConfirmedPartialAsync(Guid transactionId, ulong afterPosition, CancellationToken cancellationToken)
    {
        if (_hasMoreTransactionEventsAsync is null)
        {
            return false;
        }
        try
        {
            using var timeout = new CancellationTokenSource(VerificationTimeout, _timeProvider);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
            return !await _hasMoreTransactionEventsAsync(transactionId, afterPosition, linked.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A failed read confirms nothing, so the transaction keeps waiting and is verified again later.
            _logger.TransactionVerificationFailed(ex, transactionId, afterPosition);
            return false;
        }
    }

    private void HandlePartialTransactions(Dictionary<string, OpenTransaction> openTransactions, ulong lastPosition, string reason)
    {
        foreach (var (streamName, open) in openTransactions)
        {
            switch (_partialTransactionHandling)
            {
                case PartialTransactionHandling.Deliver:
                    _logger.DeliveringPartialTransaction(open.TransactionId, streamName, open.ReceivedCount, open.PartitionSize, lastPosition, reason);
                    break;
                case PartialTransactionHandling.DeliverAndWarn:
                    _logger.DeliveringPartialTransactionWithWarning(open.TransactionId, streamName, open.ReceivedCount, open.PartitionSize, lastPosition, reason);
                    break;
                default:
                    // Nothing of the transaction has been handed off or checkpointed, so it is read again on restart.
                    _logger.HaltingOnPartialTransaction(open.TransactionId, streamName, open.ReceivedCount, open.PartitionSize, lastPosition, reason);
                    throw new KurrentDBPartialTransactionException(open.TransactionId, streamName, lastPosition, open.ReceivedCount, open.PartitionSize);
            }
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

	[LoggerMessage(Level = LogLevel.Debug, Message = "Delivering partial transaction {TransactionId} in stream {StreamName} with {ReceivedCount} of {PartitionSize} events ending at position {Position} because {Reason}; the rest were filtered out.")]
	public static partial void DeliveringPartialTransaction(this ILogger logger, Guid transactionId, string streamName, int receivedCount, int partitionSize, ulong position, string reason);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Delivering partial transaction {TransactionId} in stream {StreamName} with {ReceivedCount} of {PartitionSize} events ending at position {Position} because {Reason}; the rest were filtered out.")]
	public static partial void DeliveringPartialTransactionWithWarning(this ILogger logger, Guid transactionId, string streamName, int receivedCount, int partitionSize, ulong position, string reason);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Halting on partial transaction {TransactionId} in stream {StreamName} with {ReceivedCount} of {PartitionSize} events ending at position {Position} because {Reason}. Change the subscription filter or PartialTransactionHandling and restart.")]
	public static partial void HaltingOnPartialTransaction(this ILogger logger, Guid transactionId, string streamName, int receivedCount, int partitionSize, ulong position, string reason);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Could not verify whether transaction {TransactionId} continues after position {Position}; it keeps waiting and will be verified again.")]
	public static partial void TransactionVerificationFailed(this ILogger logger, Exception exception, Guid transactionId, ulong position);
}
