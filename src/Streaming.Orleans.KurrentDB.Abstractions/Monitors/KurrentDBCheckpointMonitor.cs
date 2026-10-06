using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Continuum.Streaming.Orleans.KurrentDB.Monitors;

public sealed class KurrentDBCheckpointMonitorOptions
{
    [Required, Range(1, 1_000_000)]
    public uint KurrentDBCheckpointInterval { get; set; } = 480; // 32 X KurrentDBCheckpointInterval (32 x 469 = 15_008)

    [Required, Range(1, 1_000_000)]
    public ulong MaxEventsBeforeCommit { get; set; } = 10000;

    [Required, Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan MaxTimeBeforeCommit { get; set; } = TimeSpan.FromSeconds(20);
}

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

    public ValueTask<bool> TryCheckpointAsync(IStreamedEvent<object> streamedEvent, CancellationToken cancellationToken = default)
    {
        ulong position = _streamName.IsAllStream
            ? StreamSequenceNumber.ToCommitPosition(streamedEvent.SequenceNumber, streamedEvent.StreamName)
            : streamedEvent.StreamPosition;

        return TryCheckpointAsync(position, cancellationToken);
    }

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
