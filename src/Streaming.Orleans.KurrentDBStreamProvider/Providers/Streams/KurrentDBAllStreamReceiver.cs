using System.Collections.Concurrent;
using System.Diagnostics;

using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Streaming.KurrentDBStorage;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Subscribes to the KurrentDB <c>$all</c> stream with a client managed read position and receives data from an
///     internal queue.
/// </summary>
/// <remarks>
///     <para>
///         Unlike a persistent subscription, the server keeps no state for this strategy. The read position is owned by
///         the client, persisted through <see cref="KurrentDBAllStreamReceiverOptions.CheckpointStore" /> and throttled
///         by <see cref="KurrentDBAllStreamReceiverOptions.CheckpointMonitorOptions" />.
///     </para>
///     <para>
///         The subscription is pumped by a background task that buffers events, and <see cref="Receive" /> drains that
///         buffer. Checkpoints are only recorded for events that were actually handed to Orleans, so a restart replays
///         from the last delivered event rather than from the last event the server pushed.
///     </para>
/// </remarks>
public class KurrentDBAllStreamReceiver : IKurrentDBReceiver
{
    private const int ReconnectDelayMilliseconds = 1000;

    private readonly KurrentDBReceiverSettings _settings;
    private readonly KurrentDBAllStreamReceiverOptions _receiverOptions;
    private readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    private readonly UserCredentials? _credentials;
    private readonly ILogger _logger;
    private readonly string _subscriptionName;

    private readonly ConcurrentDictionary<Uuid, bool> _hashSet = new();

    // The carrier holds the $all position in SequenceNumber because it cannot be recovered from the event record
    // itself once link resolution is enabled.
    private readonly ConcurrentQueue<StreamedEvent<KurrentDBRawEvent>> _queue = new();

    // NOTE: this client is a keyed singleton owned by the container and shared by every queue of this connection.
    // It must never be disposed by this receiver.
    private readonly KurrentDBClient _client;

    private CancellationTokenSource? _cancellation;
    private Task? _pump;
    private bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBAllStreamReceiver" />.
    /// </summary>
    /// <param name="client">The shared KurrentDB client for this connection.</param>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="position">
    ///     The Orleans queue checkpointer position. Unused by this strategy, see the remarks.
    /// </param>
    /// <param name="logger">The logger.</param>
    /// <remarks>
    ///     <paramref name="position" /> is deliberately ignored. The Orleans queue checkpointer stores an
    ///     <c>EventNumber</c>, which is a per-stream revision, not a global log position. Interpreting it as a
    ///     <see cref="Position" /> would resume <c>$all</c> from near the start of the log and replay almost
    ///     everything. The read position for this strategy comes from
    ///     <see cref="KurrentDBAllStreamReceiverOptions.CheckpointStore" /> instead, which holds real commit positions.
    /// </remarks>
    public KurrentDBAllStreamReceiver(KurrentDBClient client, KurrentDBReceiverSettings settings, string position, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(settings.Options, nameof(settings.Options));
        ArgumentNullException.ThrowIfNull(settings.ReceiverOptions, nameof(settings.ReceiverOptions));
        ArgumentException.ThrowIfNullOrEmpty(settings.QueueName, nameof(settings.QueueName));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        if (settings.ReceiverOptions is not KurrentDBAllStreamReceiverOptions receiverOptions)
        {
            throw new ArgumentException($"{nameof(KurrentDBAllStreamReceiver)} requires {nameof(KurrentDBAllStreamReceiverOptions)}, but received {settings.ReceiverOptions.GetType().Name}.", nameof(settings));
        }
        ArgumentNullException.ThrowIfNull(receiverOptions.CheckpointStore, nameof(receiverOptions.CheckpointStore));
        ArgumentNullException.ThrowIfNull(receiverOptions.CheckpointMonitorOptions, nameof(receiverOptions.CheckpointMonitorOptions));
        _client = client;
        _settings = settings;
        _receiverOptions = receiverOptions;
        _credentials = settings.Options.Credentials?.ToUserCredentials();
        _logger = logger;

        // The $all subscription is validated to be the only queue of this provider, so the queue name uniquely
        // identifies the checkpoint stream of this stream provider.
        _subscriptionName = $"{settings.Options.Name}-{settings.QueueName}";
        _checkpointMonitor = new KurrentDBCheckpointMonitor(logger, receiverOptions.CheckpointMonitorOptions, receiverOptions.CheckpointStore, _subscriptionName, StreamName.AllStream);
    }

    /// <summary>
    ///     Subscribes to the <c>$all</c> stream
    /// </summary>
    public async Task InitAsync()
    {
        var timer = Stopwatch.StartNew();
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("KurrentDBAllStreamReceiver for queue {QueueName} is initializing.", _settings.QueueName);
            }
            var startPosition = await GetStartPositionAsync().ConfigureAwait(false);
            _cancellation = new CancellationTokenSource();
            _pump = Task.Run(() => PumpAsync(startPosition, _cancellation.Token));
            _initialized = true;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.LogDebug("InitAsync: QueueName={QueueName}, initialized in {ElapsedMilliseconds} ms", _settings.QueueName, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, "InitAsync: QueueName={QueueName}, errored in {ElapsedMilliseconds} ms.", _settings.QueueName, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to init KurrentDB all-stream subscription, {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <summary>
    ///     Stops the subscription and commits any pending checkpoint.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_initialized == false)
        {
            return;
        }
        try
        {
            if (_cancellation is not null)
            {
                await _cancellation.CancelAsync().ConfigureAwait(false);
            }
            if (_pump is not null)
            {
                await _pump.ConfigureAwait(false);
            }
            // Persist the position of the last event handed to Orleans so a restart does not replay it.
            await _checkpointMonitor.CloseAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CloseAsync: QueueName={QueueName}", _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to close KurrentDB all-stream subscription, {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            // NOTE: only the subscription is stopped. The KurrentDB client is a container owned keyed singleton shared
            // by every queue of this connection; disposing it here would break the other receivers.
            _cancellation?.Dispose();
            _cancellation = null;
            _pump = null;
            _initialized = false;
        }
    }

    /// <summary>
    ///     Asking for more messages from internal queue.
    /// </summary>
    /// <param name="maxCount">Max amount of message which should be delivered in this request</param>
    /// <remarks>
    ///     No checkpoint is written here. Draining the buffer only moves the events into the cache, ahead of any
    ///     consumer reading them, so checkpointing at this point would lose messages if the silo stopped before
    ///     delivery. The checkpoint is written once the cache reports the events as purged behind every active cursor.
    /// </remarks>
    public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Receiving events from internal queue: {QueueName}", _settings.QueueName);
        }
        if (_initialized == false)
        {
            return new List<StreamedEvent<KurrentDBRawEvent>>();
        }
        try
        {
            var streamedEvents = new List<StreamedEvent<KurrentDBRawEvent>>();
            for (var i = 0; i < maxCount; i++)
            {
                if (!_queue.TryDequeue(out var streamedEvent))
                {
                    break;
                }
                streamedEvents.Add(streamedEvent);
                // ToSequentialGuid is the inverse of the FromSequentialGuid used to build the carrier; ToGuid uses a
                // different byte order and would never match the key the pump added.
                _hashSet.TryRemove(Uuid.FromGuid(streamedEvent.EventId.ToSequentialGuid()), out _);
            }
            return streamedEvents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive events from internal queue for queue {QueueName}.", _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to receive events from internal queue for queue {_settings.QueueName}. {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task MessagesDeliveredAsync(ulong commitPosition)
    {
        if (_initialized == false)
        {
            return;
        }
        // This is the only safe point to advance the checkpoint. The monitor throttles the actual write and ignores
        // positions that do not move forward.
        await _checkpointMonitor.TryCheckpointAsync(commitPosition).ConfigureAwait(false);
    }

    #region Subscription

    private async Task<FromAll> GetStartPositionAsync()
    {
        // Only the checkpoint store holds genuine $all commit positions; see the constructor remarks for why the
        // Orleans queue checkpointer position is not usable here.
        var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync().ConfigureAwait(false);
        if (lastCheckpoint.HasValue)
        {
            _logger.LogInformation("Resuming KurrentDB $all subscription {SubscriptionName} after checkpointed position {Position}.", _subscriptionName, lastCheckpoint.Value);
            return FromAll.After(new Position(lastCheckpoint.Value, lastCheckpoint.Value));
        }
        if (_receiverOptions.StartFromNow)
        {
            _logger.LogInformation("Starting to read latest messages from KurrentDB $all for queue {ConnectionName}-{QueueName}.", _settings.Options.Name, _settings.QueueName);
            return FromAll.End;
        }
        _logger.LogInformation("Starting to read messages from beginning of KurrentDB $all for queue {ConnectionName}-{QueueName}.", _settings.Options.Name, _settings.QueueName);
        return FromAll.Start;
    }

    private SubscriptionFilterOptions? GetFilterOptions()
    {
        if (string.IsNullOrWhiteSpace(_receiverOptions.StreamFilterPrefix))
        {
            return null;
        }
        // The checkpoint interval controls how often the server reports progress while the filter excludes events,
        // which keeps a heavily filtered subscription from appearing stalled.
        return new SubscriptionFilterOptions(StreamFilter.Prefix(_receiverOptions.StreamFilterPrefix), _receiverOptions.CheckpointMonitorOptions.KurrentDBCheckpointInterval);
    }

    private async Task PumpAsync(FromAll startPosition, CancellationToken cancellationToken)
    {
        var filterOptions = GetFilterOptions();
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await using var subscription = _client.SubscribeToAll(startPosition, _receiverOptions.ResolveLinkTos, filterOptions, _credentials, cancellationToken);
                await foreach (var message in subscription.Messages.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    switch (message)
                    {
                        case StreamMessage.Event(var resolvedEvent):
                            // OriginalPosition is the position of the event actually read from $all. With link
                            // resolution enabled, resolvedEvent.Event is the linked-to event, which sits at an
                            // unrelated position in the log and would rewind or skip the subscription if used here.
                            var allStreamPosition = resolvedEvent.OriginalPosition?.CommitPosition;
                            if (allStreamPosition is null)
                            {
                                // A $all subscription always reports a position. Skipping is safer than guessing one.
                                _logger.LogWarning("KurrentDB $all subscription {SubscriptionName} received event {EventId} without an original position. Skipping it.", _subscriptionName, resolvedEvent.Event.EventId);
                                break;
                            }
                            // Check duplication
                            if (_hashSet.TryAdd(resolvedEvent.Event.EventId, true))
                            {
                                _queue.Enqueue(KurrentDBStreamedEventFactory.FromAllStream(resolvedEvent));
                            }
                            // Resuming a dropped subscription must continue from the last event seen by the pump,
                            // otherwise the buffered but undelivered events would be received twice.
                            startPosition = FromAll.After(resolvedEvent.OriginalPosition.Value);
                            break;

                        case StreamMessage.SubscriptionConfirmation:
                            _logger.LogInformation("KurrentDB $all subscription {SubscriptionName} confirmed.", _subscriptionName);
                            break;

                        case StreamMessage.CaughtUp:
                            _logger.LogInformation("KurrentDB $all subscription {SubscriptionName} has caught up to the head of the stream.", _subscriptionName);
                            break;

                        case StreamMessage.FellBehind:
                            _logger.LogWarning("KurrentDB $all subscription {SubscriptionName} has fallen behind.", _subscriptionName);
                            break;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "KurrentDB $all subscription {SubscriptionName} was dropped. Resubscribing.", _subscriptionName);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            try
            {
                await Task.Delay(ReconnectDelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    #endregion
}
