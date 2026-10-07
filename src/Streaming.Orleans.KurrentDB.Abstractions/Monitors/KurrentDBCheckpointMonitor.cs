using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using Continuum.Streaming.Orleans;

namespace Continuum.Streaming.Orleans.KurrentDB.Monitors;

/// <summary>
///     Controls how often a subscription commits its checkpoint.
/// </summary>
public sealed class KurrentDBCheckpointMonitorOptions
{
    /// <summary>
    ///     The interval, in filtered events, at which KurrentDB reports a checkpoint to a filtered subscription.
    /// </summary>
    [Required, Range(1, 1_000_000)]
    public uint KurrentDBCheckpointInterval { get; set; } = 480; // 32 X KurrentDBCheckpointInterval (32 x 469 = 15_008)

    /// <summary>
    ///     The number of processed events after which the checkpoint is committed.
    /// </summary>
    [Required, Range(1, 1_000_000)]
    public ulong MaxEventsBeforeCommit { get; set; } = 10000;

    /// <summary>
    ///     The time after which a pending checkpoint is committed.
    /// </summary>
    [Required, TimeSpanRange("00:00:01", "1.00:00:00")]
    public TimeSpan MaxTimeBeforeCommit { get; set; } = TimeSpan.FromSeconds(20);
}

/// <summary>
///     Tracks a subscription's position and commits it to an <see cref="ICheckpointStore{T}" /> when due.
/// </summary>
public sealed class KurrentDBCheckpointMonitor
{
    private readonly ILogger _logger;
    private readonly KurrentDBCheckpointMonitorOptions _options;
    private readonly ICheckpointStore<ulong> _checkpointStore;
    private readonly string _subscriptionName;
    private readonly StreamName _streamName;
    private readonly Stopwatch _timer;

    private bool _currentPositionNotCommitted;
    private ulong? _currentPosition;
    private ulong _eventsProcessedSinceLastCommit;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCheckpointMonitor" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The commit cadence options.</param>
    /// <param name="checkpointStore">The store checkpoints are committed to.</param>
    /// <param name="subscriptionName">The subscription whose checkpoint is tracked.</param>
    /// <param name="streamName">The stream the subscription reads.</param>
    public KurrentDBCheckpointMonitor(ILogger? logger,
        KurrentDBCheckpointMonitorOptions? options,
        ICheckpointStore<ulong>? checkpointStore,
        string? subscriptionName,
        StreamName? streamName)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(checkpointStore, nameof(checkpointStore));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(subscriptionName, nameof(subscriptionName));
        ArgumentNullException.ThrowIfNull(streamName, nameof(streamName));

        _logger = logger;
        _options = options;
        _checkpointStore = checkpointStore;
        _subscriptionName = subscriptionName;
        _streamName = streamName;
        _timer = Stopwatch.StartNew();

        _currentPositionNotCommitted = false;
        _currentPosition = null;
        _eventsProcessedSinceLastCommit = 0;
    }

    /// <summary>
    ///     Gets the last known checkpoint position.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The position, or <see langword="null" /> when no checkpoint has been stored.</returns>
    public async Task<ulong?> GetLastCheckpointAsync(CancellationToken cancellationToken = default)
    {
        if (_currentPosition.HasValue)
        {
            return _currentPosition;
        }

        // ICheckpointStore<ulong> cannot express 'no checkpoint', it returns the default position instead.
        // Map that back to null so callers subscribe from the start rather than from just after position 0,
        // which would silently skip the first event of the stream.
        var storedPosition = await _checkpointStore.GetLastCheckpointAsync(_subscriptionName, cancellationToken);
        _currentPosition = storedPosition == default ? null : storedPosition;
        return _currentPosition;
    }

    /// <summary>
    ///     Records an event as processed and commits the checkpoint if it is due.
    /// </summary>
    /// <param name="streamedEvent">The processed event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> if the checkpoint was committed.</returns>
    public ValueTask<bool> TryCheckpointAsync(IStreamedEvent<object> streamedEvent, CancellationToken cancellationToken = default)
    {
        ulong position = _streamName.IsAllStream
            ? StreamSequenceNumber.ToCommitPosition(streamedEvent.SequenceNumber, streamedEvent.StreamName)
            : streamedEvent.StreamPosition;

        return TryCheckpointAsync(position, cancellationToken);
    }

    /// <summary>
    ///     Records a position as processed and commits the checkpoint if it is due.
    /// </summary>
    /// <param name="position">The processed position.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true" /> if the checkpoint was committed.</returns>
    public ValueTask<bool> TryCheckpointAsync(ulong position, CancellationToken cancellationToken = default)
    {
        var hasBeenProcessed = _currentPosition.HasValue && position <= _currentPosition.Value;
        if (hasBeenProcessed)
        {
            return ValueTask.FromResult(false);
        }

        _currentPosition = position;
        _currentPositionNotCommitted = true;
        _eventsProcessedSinceLastCommit++;

        return TryCheckpointAsync(cancellationToken);
    }

    private async ValueTask<bool> TryCheckpointAsync(CancellationToken cancellationToken)
    {
        var commitNotDue = _eventsProcessedSinceLastCommit < _options.MaxEventsBeforeCommit && _timer.Elapsed < _options.MaxTimeBeforeCommit;
        if (commitNotDue)
        {
            return false;
        }

        await CommitCheckpointAsync(cancellationToken);
        Reset();

        return true;
    }

    /// <summary>
    ///     Commits any uncommitted position and stops tracking.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentPositionNotCommitted)
            {
                await CommitCheckpointAsync(cancellationToken);
            }
        }
        finally
        {
            _timer.Stop();
            _currentPositionNotCommitted = false;
            _currentPosition = null;
            _eventsProcessedSinceLastCommit = 0;
        }
    }

    private Task CommitCheckpointAsync(CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Subscription '{SubscriptionName}' for stream '{StreamName}' committing checkpoint at position '{CommitPosition}'.",
                _subscriptionName, _streamName, _currentPosition);
        }
        return _checkpointStore.StoreCheckpointAsync(_subscriptionName, _currentPosition!.Value, cancellationToken);
    }

    private void Reset()
    {
        _currentPositionNotCommitted = false;
        _eventsProcessedSinceLastCommit = 0;
        _timer.Restart();
    }
}
