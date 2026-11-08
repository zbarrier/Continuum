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

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Catchup;

/// <summary>
///     Subscribes to the KurrentDB <c>$all</c> stream or to a single stream with a client managed read position and receives data from an
///     internal queue.
/// </summary>
/// <remarks>
///     <para>
///         Unlike a persistent subscription, the server keeps no state for this strategy. The read position is owned by
///         the client, persisted through <see cref="KurrentDBCatchupSubscriptionReceiverOptions.CheckpointStore" /> and throttled
///         by <see cref="KurrentDBCatchupSubscriptionReceiverOptions.CheckpointMonitorOptions" />.
///     </para>
///     <para>
///         The subscription is pumped by a background task that buffers events, and <see cref="Receive" /> drains that
///         buffer. Checkpoints are only recorded for events that were actually handed to Orleans, so a restart replays
///         from the last delivered event rather than from the last event the server pushed.
///     </para>
/// </remarks>
public class KurrentDBCatchupSubscriptionReceiver : IKurrentDBCatchupReceiver
{

    private readonly KurrentDBReceiverSettings _settings;
    private readonly KurrentDBCatchupSubscriptionReceiverOptions _receiverOptions;
    // Set only for $all subscriptions. A single-stream subscription reads the stream named by the queue.
    private readonly KurrentDBAllStreamCatchupSubscriptionReceiverOptions? _allStreamOptions;
    private readonly string _streamName;
    private readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    private readonly UserCredentials? _credentials;
    private readonly ILogger _logger;
    private readonly string _subscriptionName;
    private readonly KurrentDBCheckpointMarkerData _checkpointMarker;

    // The carrier holds the $all commit position or stream revision in SequenceNumber because it cannot be recovered from the event record
    // itself once link resolution is enabled. The buffer is bounded so a catch-up from the start waits for
    // Orleans to drain it instead of reading the whole log into memory.
    private Channel<StreamedEvent<KurrentDBRawEvent>> _queue;

    internal const int MaxBufferCapacityMultiplier = int.MaxValue / KurrentDBCatchupQueueAdapterReceiver.MaxMessagesPerRead;

    internal int BufferCapacity => _receiverOptions.BufferCapacityMultiplier * KurrentDBCatchupQueueAdapterReceiver.MaxMessagesPerRead;

    internal int BufferedCount => _queue.Reader.Count;

    private Channel<StreamedEvent<KurrentDBRawEvent>> CreateBuffer() =>
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
    // Written by init and close, read by the pulling agent and producer callers on other threads.
    private volatile bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBCatchupSubscriptionReceiver" />.
    /// </summary>
    /// <param name="client">The shared KurrentDB client for this connection.</param>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="checkpointMarker">
    ///     The serialized checkpoint marker enqueued when a filtered <c>$all</c> subscription reaches a checkpoint. A single-stream\r\n    ///     subscription never enqueues markers.
    /// </param>
    /// <remarks>
    ///     The read position comes from <see cref="KurrentDBCatchupSubscriptionReceiverOptions.CheckpointStore" />, which holds
    ///     <c>$all</c> commit positions or, for a single-stream subscription, stream revisions.
    /// </remarks>
    internal KurrentDBCatchupSubscriptionReceiver(KurrentDBClient client, KurrentDBReceiverSettings settings, ILogger logger, KurrentDBCheckpointMarkerData checkpointMarker)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(settings.Options, nameof(settings.Options));
        ArgumentNullException.ThrowIfNull(settings.ReceiverOptions, nameof(settings.ReceiverOptions));
        ArgumentException.ThrowIfNullOrEmpty(settings.QueueName, nameof(settings.QueueName));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(checkpointMarker, nameof(checkpointMarker));
        if (settings.ReceiverOptions is not KurrentDBCatchupSubscriptionReceiverOptions receiverOptions)
        {
            throw new ArgumentException($"{nameof(KurrentDBCatchupSubscriptionReceiver)} requires {nameof(KurrentDBCatchupSubscriptionReceiverOptions)}, but received {settings.ReceiverOptions.GetType().Name}.", nameof(settings));
        }
        ArgumentNullException.ThrowIfNull(receiverOptions.CheckpointStore, nameof(receiverOptions.CheckpointStore));
        ArgumentNullException.ThrowIfNull(receiverOptions.CheckpointMonitorOptions, nameof(receiverOptions.CheckpointMonitorOptions));
        ArgumentOutOfRangeException.ThrowIfLessThan(receiverOptions.BufferCapacityMultiplier, 1, nameof(receiverOptions.BufferCapacityMultiplier));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(receiverOptions.BufferCapacityMultiplier, MaxBufferCapacityMultiplier, nameof(receiverOptions.BufferCapacityMultiplier));
        _client = client;
        _settings = settings;
        _receiverOptions = receiverOptions;
        _allStreamOptions = receiverOptions as KurrentDBAllStreamCatchupSubscriptionReceiverOptions;
        _streamName = _allStreamOptions is null ? settings.QueueName : KurrentDBStreams.All;
        _queue = CreateBuffer();
        _credentials = settings.Options.Credentials?.ToUserCredentials();
        _logger = logger;
        _checkpointMarker = checkpointMarker;

        // A catch-up provider has exactly one queue, so the checkpoint belongs to the provider. The consumer group is the
        // service id and provider name, which Orleans keeps unique, so several catch-up providers on the same connection,
        // and other services sharing the checkpoint store, never share a checkpoint. The connection name comes first
        // because positions are only meaningful for the database they were read from.
        _subscriptionName = $"{settings.Options.ConnectionName}-{settings.ConsumerGroup}";
        _checkpointMonitor = new KurrentDBCheckpointMonitor(logger, receiverOptions.CheckpointMonitorOptions, receiverOptions.CheckpointStore, _subscriptionName, _streamName);
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
            var cancellationToken = _cancellation.Token;
            _pump = _allStreamOptions is null
                ? Task.Run(() => PumpStreamAsync(startPosition, cancellationToken))
                : Task.Run(() => PumpAllAsync(startPosition, cancellationToken));
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
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to init KurrentDB catch-up subscription, {ex.GetType()}: {ex.Message}"), ex);
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
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to close KurrentDB catch-up subscription, {ex.GetType()}: {ex.Message}"), ex);
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
    public IReadOnlyList<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
    {
        _logger.ReceivingEvents(_settings.QueueName);
        if (_initialized == false)
        {
            return Array.Empty<StreamedEvent<KurrentDBRawEvent>>();
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
            }
            return streamedEvents;
        }
        catch (Exception ex)
        {
            _logger.ReceiveEventsFailed(ex, _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to receive events from internal queue for queue {_settings.QueueName}. {ex.GetType()}: {ex.Message}"), ex);
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
        // positions that do not move forward. $all positions and stream revisions are monotonic, so every pending position up to this one
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

    /// <inheritdoc />
    public async Task<bool> HasMoreTransactionEventsAsync(Guid transactionId, ulong afterPosition, CancellationToken cancellationToken)
    {
        // Read with the subscription's own filter, so an event the filter excludes cannot be mistaken for the rest of the
        // transaction. The read is bounded by the caller's token; failing it confirms nothing.
        if (_allStreamOptions is not null)
        {
            var from = new Position(afterPosition, afterPosition);
            var events = _allStreamOptions.Filter is { } filter
                ? _client.ReadAllAsync(Direction.Forwards, from, filter, maxCount: 2, resolveLinkTos: _receiverOptions.ResolveLinkTos, userCredentials: _credentials, cancellationToken: cancellationToken)
                : _client.ReadAllAsync(Direction.Forwards, from, maxCount: 2, resolveLinkTos: _receiverOptions.ResolveLinkTos, userCredentials: _credentials, cancellationToken: cancellationToken);
            await foreach (var resolvedEvent in ((IAsyncEnumerable<ResolvedEvent>)events).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                // The read starts at the last event received, which is returned again.
                if (resolvedEvent.OriginalPosition is not { } position || position.CommitPosition <= afterPosition || resolvedEvent.Event is null)
                {
                    continue;
                }
                return KurrentDBCatchupStreamedEventFactory.FromAllStream(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser).Metadata.TransactionId == transactionId;
            }
            return false;
        }
        var readResult = _client.ReadStreamAsync(Direction.Forwards, _streamName, new StreamPosition(afterPosition + 1), maxCount: 1, resolveLinkTos: _receiverOptions.ResolveLinkTos, userCredentials: _credentials, cancellationToken: cancellationToken);
        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            return false;
        }
        await foreach (var resolvedEvent in ((IAsyncEnumerable<ResolvedEvent>)readResult).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return resolvedEvent.Event is not null
                && KurrentDBCatchupStreamedEventFactory.FromStream(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser).Metadata.TransactionId == transactionId;
        }
        return false;
    }

    /// <summary>
    ///     Returns the position to subscribe after, or <see langword="null" /> to subscribe from the start. For
    ///     <c>$all</c> this is a commit position; for a single stream it is a stream revision.
    /// </summary>
    private async Task<ulong?> GetStartPositionAsync()
    {
        // Only the checkpoint store holds the subscription's positions.
        var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync().ConfigureAwait(false);
        if (lastCheckpoint == KurrentDBCheckpointMonitor.StreamStartPosition && _allStreamOptions is null)
        {
            _logger.StartingFromBeginning(_settings.Options.Name, _settings.QueueName);
            return null;
        }
        if (lastCheckpoint.HasValue)
        {
            _logger.ResumingSubscription(_subscriptionName, lastCheckpoint.Value);
            return lastCheckpoint.Value;
        }
        if (_receiverOptions.StartFromNow && await ReadLastPositionAsync().ConfigureAwait(false) is { } lastPosition)
        {
            // Starting from the current end is turned into a real position and committed right away. Subscribing from
            // the end instead would pick a new end on every reconnect or restart before the first checkpoint,
            // skipping whatever was written in between.
            _logger.StartingFromLatest(_settings.Options.Name, _settings.QueueName, lastPosition);
            await _checkpointMonitor.CommitInitialCheckpointAsync(lastPosition).ConfigureAwait(false);
            return lastPosition;
        }
        // An empty database or missing stream has nothing to skip, so starting from now and from the beginning are the same.
        _logger.StartingFromBeginning(_settings.Options.Name, _settings.QueueName);
        if (_receiverOptions.StartFromNow && _allStreamOptions is null)
        {
            // A missing stream is committed as starting before its first event, so a restart before the first regular
            // commit does not mistake the events written meanwhile for history to skip.
            await _checkpointMonitor.CommitInitialCheckpointAsync(KurrentDBCheckpointMonitor.StreamStartPosition).ConfigureAwait(false);
        }
        return null;
    }

    private async Task<ulong?> ReadLastPositionAsync()
    {
        if (_allStreamOptions is not null)
        {
            await foreach (var resolvedEvent in ((IAsyncEnumerable<ResolvedEvent>)_client.ReadAllAsync(Direction.Backwards, Position.End, maxCount: 1, resolveLinkTos: false, userCredentials: _credentials)).ConfigureAwait(false))
            {
                return resolvedEvent.OriginalPosition?.CommitPosition;
            }
            return null;
        }
        var readResult = _client.ReadStreamAsync(Direction.Backwards, _streamName, StreamPosition.End, maxCount: 1, resolveLinkTos: false, userCredentials: _credentials);
        if (await readResult.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            return null;
        }
        await foreach (var resolvedEvent in ((IAsyncEnumerable<ResolvedEvent>)readResult).ConfigureAwait(false))
        {
            return resolvedEvent.OriginalEventNumber.ToUInt64();
        }
        return null;
    }

    private SubscriptionFilterOptions? GetFilterOptions(KurrentDBAllStreamCatchupSubscriptionReceiverOptions allStreamOptions)
    {
        if (allStreamOptions.Filter is null)
        {
            return null;
        }
        // The checkpoint interval controls how often the server reports progress while the filter excludes events,
        // which keeps a heavily filtered subscription from appearing stalled.
        return new SubscriptionFilterOptions(allStreamOptions.Filter, allStreamOptions.CheckpointMonitorOptions.KurrentDBCheckpointInterval);
    }

    private async Task PumpStreamAsync(ulong? afterRevision, CancellationToken cancellationToken)
    {
        var startPosition = afterRevision.HasValue ? FromStream.After(new StreamPosition(afterRevision.Value)) : FromStream.Start;
        var reconnectAttempt = 0;
        while (cancellationToken.IsCancellationRequested == false)
        {
            try
            {
                await using var subscription = _client.SubscribeToStream(_streamName, startPosition, _receiverOptions.ResolveLinkTos, _credentials, cancellationToken);
                await foreach (var message in subscription.Messages.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    switch (message)
                    {
                        case StreamMessage.Event(var resolvedEvent):
                            // A link whose target was deleted resolves to nothing. It is skipped, but the position
                            // still moves past it so a reconnect does not read it again.
                            if (resolvedEvent.Event is not null)
                            {
                                await _queue.Writer.WriteAsync(KurrentDBCatchupStreamedEventFactory.FromStream(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser), cancellationToken).ConfigureAwait(false);
                            }
                            // Resuming a dropped subscription must continue from the last event seen by the pump,
                            // otherwise the buffered but undelivered events would be received twice.
                            startPosition = FromStream.After(resolvedEvent.OriginalEventNumber);
                            break;

                        case StreamMessage.SubscriptionConfirmation:
                            if (reconnectAttempt > 0)
                            {
                                _settings.Diagnostics?.Reconnected();
                            }
                            else
                            {
                                _settings.Diagnostics?.Running();
                            }
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
                _settings.Diagnostics?.Reconnecting(ex);
                LogSubscriptionDropped(ex, reconnectAttempt);
            }
            if (await DelayReconnectAsync(reconnectAttempt, cancellationToken).ConfigureAwait(false) == false)
            {
                return;
            }
        }
    }

    private void LogSubscriptionDropped(Exception ex, int reconnectAttempt)
    {
        var level = KurrentDBReconnectPolicy.GetLogLevel(reconnectAttempt);
        if (level is { } logLevel)
        {
            _logger.SubscriptionDropped(logLevel, ex, _subscriptionName, reconnectAttempt);
        }
    }

    private static async Task<bool> DelayReconnectAsync(int reconnectAttempt, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        try
        {
            await Task.Delay(KurrentDBReconnectPolicy.GetDelay(Math.Max(reconnectAttempt, 1), Random.Shared.NextDouble()), cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task PumpAllAsync(ulong? afterCommitPosition, CancellationToken cancellationToken)
    {
        var startPosition = afterCommitPosition.HasValue ? FromAll.After(new Position(afterCommitPosition.Value, afterCommitPosition.Value)) : FromAll.Start;
        var filterOptions = GetFilterOptions(_allStreamOptions!);
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
                            // No duplicate check: $all delivers each position once. With ResolveLinkTos, an event and a link to
                            // it are separate positions that both resolve to the same event, and both are delivered when the
                            // filter matches the original stream and the link stream. Filter to one or the other to avoid that.
                            await _queue.Writer.WriteAsync(KurrentDBCatchupStreamedEventFactory.FromAllStream(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser), cancellationToken).ConfigureAwait(false);
                            // Resuming a dropped subscription must continue from the last event seen by the pump,
                            // otherwise the buffered but undelivered events would be received twice.
                            startPosition = FromAll.After(resolvedEvent.OriginalPosition.GetValueOrDefault());
                            break;

                        case StreamMessage.AllStreamCheckpointReached(var checkpointPosition):
                            // A filtered subscription reports progress through events it did not deliver. The
                            // position is enqueued as a marker behind the events already buffered, so it is only
                            // checkpointed once the cache purges it, after every event before it was delivered.
                            await _queue.Writer.WriteAsync(KurrentDBCatchupStreamedEventFactory.CheckpointMarker(checkpointPosition.CommitPosition, _settings.Options.ConnectionName, _checkpointMarker), cancellationToken).ConfigureAwait(false);
                            startPosition = FromAll.After(checkpointPosition);
                            break;

                        case StreamMessage.SubscriptionConfirmation:
                            if (reconnectAttempt > 0)
                            {
                                _settings.Diagnostics?.Reconnected();
                            }
                            else
                            {
                                _settings.Diagnostics?.Running();
                            }
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
                _settings.Diagnostics?.Reconnecting(ex);
                LogSubscriptionDropped(ex, reconnectAttempt);
            }
            if (await DelayReconnectAsync(reconnectAttempt, cancellationToken).ConfigureAwait(false) == false)
            {
                return;
            }
        }
    }

    #endregion
}

internal static partial class KurrentDBCatchupSubscriptionReceiverLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "KurrentDBCatchupSubscriptionReceiver for queue {QueueName} is initializing.")]
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Resuming KurrentDB catch-up subscription {SubscriptionName} after checkpointed position {Position}.")]
    public static partial void ResumingSubscription(this ILogger logger, string subscriptionName, ulong position);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read latest messages from KurrentDB for queue {ConnectionName}-{QueueName} after position {Position}.")]
    public static partial void StartingFromLatest(this ILogger logger, string connectionName, string queueName, ulong position);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read messages from beginning of KurrentDB for queue {ConnectionName}-{QueueName}.")]
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
