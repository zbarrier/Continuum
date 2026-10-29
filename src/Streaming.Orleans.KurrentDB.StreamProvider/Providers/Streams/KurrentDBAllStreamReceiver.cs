using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

using Continuum.Streaming.Orleans;
using Continuum.Streaming.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider;

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

    private readonly KurrentDBReceiverSettings _settings;
    private readonly KurrentDBAllStreamReceiverOptions _receiverOptions;
    private readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    private readonly UserCredentials? _credentials;
    private readonly ILogger _logger;
    private readonly string _subscriptionName;
    private readonly KurrentDBCheckpointMarkerData _checkpointMarker;

    private readonly ConcurrentDictionary<Uuid, bool> _hashSet = new();

    // The carrier holds the $all position in SequenceNumber because it cannot be recovered from the event record
    // itself once link resolution is enabled. The buffer is bounded so a catch-up from the start of $all waits for
    // Orleans to drain it instead of reading the whole log into memory.
    private Channel<StreamedEvent<KurrentDBRawEvent>> _queue = CreateBuffer();

    internal const int BufferCapacity = 10 * KurrentDBQueueAdapterReceiver.MaxMessagesPerRead;

    internal int BufferedCount => _queue.Reader.Count;

    private static Channel<StreamedEvent<KurrentDBRawEvent>> CreateBuffer() =>
        Channel.CreateBounded<StreamedEvent<KurrentDBRawEvent>>(new BoundedChannelOptions(BufferCapacity) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });

    // Positions handed to Orleans and not yet reported as delivered. The cache reports only the newest purged
    // position, so these let the checkpoint monitor count every purged event toward its event limit.
    private readonly ConcurrentQueue<ulong> _pendingPositions = new();
    private readonly object _pendingPositionsLock = new();

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
    /// <param name="logger">The logger.</param>
    /// <param name="checkpointMarker">
    ///     The serialized checkpoint marker enqueued when a filtered subscription reaches a checkpoint.
    /// </param>
    /// <remarks>
    ///     The read position comes from <see cref="KurrentDBAllStreamReceiverOptions.CheckpointStore" />, which holds
    ///     <c>$all</c> commit positions.
    /// </remarks>
    internal KurrentDBAllStreamReceiver(KurrentDBClient client, KurrentDBReceiverSettings settings, ILogger logger, KurrentDBCheckpointMarkerData checkpointMarker)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(settings.Options, nameof(settings.Options));
        ArgumentNullException.ThrowIfNull(settings.ReceiverOptions, nameof(settings.ReceiverOptions));
        ArgumentException.ThrowIfNullOrEmpty(settings.QueueName, nameof(settings.QueueName));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(checkpointMarker, nameof(checkpointMarker));
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
        _checkpointMarker = checkpointMarker;

        // A $all provider has exactly one queue, so the checkpoint belongs to the provider. The consumer group is the
        // service id and provider name, which Orleans keeps unique, so several $all providers on the same connection,
        // and other services sharing the checkpoint store, never share a checkpoint. The connection name comes first
        // because positions are only meaningful for the database they were read from.
        _subscriptionName = $"{settings.Options.ConnectionName}-{settings.ConsumerGroup}";
        _checkpointMonitor = new KurrentDBCheckpointMonitor(logger, receiverOptions.CheckpointMonitorOptions, receiverOptions.CheckpointStore, _subscriptionName, KurrentDBStreams.All);
    }

    /// <summary>
    ///     Subscribes to the <c>$all</c> stream
    /// </summary>
    public async Task InitAsync()
    {
        var timer = Stopwatch.StartNew();
        try
        {
            _logger.ReceiverInitializing(_settings.QueueName);
            var startPosition = await GetStartPositionAsync().ConfigureAwait(false);
            _cancellation = new CancellationTokenSource();
            _queue = CreateBuffer();
            _pump = Task.Run(() => PumpAsync(startPosition, _cancellation.Token));
            _initialized = true;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.ReceiverInitialized(_settings.QueueName, timer.Elapsed.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.ReceiverInitializationFailed(ex, _settings.QueueName, timer.Elapsed.TotalMilliseconds);
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
            _logger.ReceiverCloseFailed(ex, _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to close KurrentDB all-stream subscription, {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            // NOTE: only the subscription is stopped. The KurrentDB client is a container owned keyed singleton shared
            // by every queue of this connection; disposing it here would break the other receivers.
            _cancellation?.Dispose();
            _cancellation = null;
            _pump = null;
            _pendingPositions.Clear();
            // Buffered events were never handed to Orleans, so the next subscription rereads them from the checkpoint.
            _queue.Writer.TryComplete();
            while (_queue.Reader.TryRead(out _))
            {
            }
            _hashSet.Clear();
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
        _logger.ReceivingEvents(_settings.QueueName);
        if (_initialized == false)
        {
            return new List<StreamedEvent<KurrentDBRawEvent>>();
        }
        try
        {
            var streamedEvents = new List<StreamedEvent<KurrentDBRawEvent>>();
            for (var i = 0; i < maxCount; i++)
            {
                if (!_queue.Reader.TryRead(out var streamedEvent))
                {
                    break;
                }
                streamedEvents.Add(streamedEvent);
                _pendingPositions.Enqueue((ulong)streamedEvent.Cursor.SequenceNumber);
                _hashSet.TryRemove(Uuid.FromGuid(streamedEvent.EventId), out _);
            }
            return streamedEvents;
        }
        catch (Exception ex)
        {
            _logger.ReceiveEventsFailed(ex, _settings.QueueName);
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
        // positions that do not move forward. $all positions are monotonic, so every pending position up to this one
        // has been purged too. The peek and dequeue run under a lock so overlapping calls cannot dequeue each other's
        // entries.
        var eventCount = 0;
        lock (_pendingPositionsLock)
        {
            while (_pendingPositions.TryPeek(out var pending) && pending <= commitPosition)
            {
                _pendingPositions.TryDequeue(out _);
                eventCount++;
            }
        }
        await _checkpointMonitor.TryCheckpointAsync(commitPosition, eventCount).ConfigureAwait(false);
    }

    #region Subscription

    private async Task<FromAll> GetStartPositionAsync()
    {
        // Only the checkpoint store holds $all commit positions.
        var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync().ConfigureAwait(false);
        if (lastCheckpoint.HasValue)
        {
            _logger.ResumingSubscription(_subscriptionName, lastCheckpoint.Value);
            return FromAll.After(new Position(lastCheckpoint.Value, lastCheckpoint.Value));
        }
        if (_receiverOptions.StartFromNow)
        {
            _logger.StartingFromLatest(_settings.Options.Name, _settings.QueueName);
            return FromAll.End;
        }
        _logger.StartingFromBeginning(_settings.Options.Name, _settings.QueueName);
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
        var reconnectAttempt = 0;
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
                                _logger.EventWithoutPosition(_subscriptionName, resolvedEvent.Event.EventId);
                                break;
                            }
                            // Check duplication
                            if (_hashSet.TryAdd(resolvedEvent.Event.EventId, true))
                            {
                                await _queue.Writer.WriteAsync(KurrentDBStreamedEventFactory.FromAllStream(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser), cancellationToken).ConfigureAwait(false);
                            }
                            // Resuming a dropped subscription must continue from the last event seen by the pump,
                            // otherwise the buffered but undelivered events would be received twice.
                            startPosition = FromAll.After(resolvedEvent.OriginalPosition.GetValueOrDefault());
                            break;

                        case StreamMessage.AllStreamCheckpointReached(var checkpointPosition):
                            // A filtered subscription reports progress through events it did not deliver. The
                            // position is enqueued as a marker behind the events already buffered, so it is only
                            // checkpointed once the cache purges it, after every event before it was delivered.
                            await _queue.Writer.WriteAsync(KurrentDBStreamedEventFactory.CheckpointMarker(checkpointPosition.CommitPosition, _settings.Options.ConnectionName, _checkpointMarker), cancellationToken).ConfigureAwait(false);
                            startPosition = FromAll.After(checkpointPosition);
                            break;

                        case StreamMessage.SubscriptionConfirmation:
                            reconnectAttempt = 0;
                            _logger.SubscriptionConfirmed(_subscriptionName);
                            break;

                        case StreamMessage.CaughtUp:
                            _logger.SubscriptionCaughtUp(_subscriptionName);
                            break;

                        case StreamMessage.FellBehind:
                            _logger.SubscriptionFellBehind(_subscriptionName);
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
                reconnectAttempt++;
                var level = KurrentDBReconnectPolicy.GetLogLevel(reconnectAttempt);
                if (level is { } logLevel)
                {
                    _logger.SubscriptionDropped(logLevel, ex, _subscriptionName, reconnectAttempt);
                }
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            try
            {
                await Task.Delay(KurrentDBReconnectPolicy.GetDelay(Math.Max(reconnectAttempt, 1), Random.Shared.NextDouble()), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    #endregion
}

internal static partial class KurrentDBAllStreamReceiverLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "KurrentDBAllStreamReceiver for queue {QueueName} is initializing.")]
    public static partial void ReceiverInitializing(this ILogger logger, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "InitAsync: QueueName={QueueName}, initialized in {ElapsedMilliseconds} ms")]
    public static partial void ReceiverInitialized(this ILogger logger, string queueName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "InitAsync: QueueName={QueueName}, errored in {ElapsedMilliseconds} ms.")]
    public static partial void ReceiverInitializationFailed(this ILogger logger, Exception exception, string queueName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "CloseAsync: QueueName={QueueName}")]
    public static partial void ReceiverCloseFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Receiving events from internal queue: {QueueName}")]
    public static partial void ReceivingEvents(this ILogger logger, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to receive events from internal queue for queue {QueueName}.")]
    public static partial void ReceiveEventsFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Resuming KurrentDB $all subscription {SubscriptionName} after checkpointed position {Position}.")]
    public static partial void ResumingSubscription(this ILogger logger, string subscriptionName, ulong position);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read latest messages from KurrentDB $all for queue {ConnectionName}-{QueueName}.")]
    public static partial void StartingFromLatest(this ILogger logger, string connectionName, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read messages from beginning of KurrentDB $all for queue {ConnectionName}-{QueueName}.")]
    public static partial void StartingFromBeginning(this ILogger logger, string connectionName, string queueName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "KurrentDB $all subscription {SubscriptionName} received event {EventId} without an original position. Skipping it.")]
    public static partial void EventWithoutPosition(this ILogger logger, string subscriptionName, Uuid eventId);

    [LoggerMessage(Level = LogLevel.Information, Message = "KurrentDB $all subscription {SubscriptionName} confirmed.")]
    public static partial void SubscriptionConfirmed(this ILogger logger, string subscriptionName);

    [LoggerMessage(Level = LogLevel.Information, Message = "KurrentDB $all subscription {SubscriptionName} has caught up to the head of the stream.")]
    public static partial void SubscriptionCaughtUp(this ILogger logger, string subscriptionName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "KurrentDB $all subscription {SubscriptionName} has fallen behind.")]
    public static partial void SubscriptionFellBehind(this ILogger logger, string subscriptionName);

    [LoggerMessage(Message = "KurrentDB $all subscription {SubscriptionName} was dropped. Reconnect attempt {Attempt}.")]
    public static partial void SubscriptionDropped(this ILogger logger, LogLevel level, Exception exception, string subscriptionName, int attempt);
}
