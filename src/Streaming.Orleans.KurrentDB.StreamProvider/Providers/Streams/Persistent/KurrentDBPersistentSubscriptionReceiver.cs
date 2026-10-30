using System.Collections.Concurrent;
using System.Diagnostics;

using Continuum;
using Continuum.Streaming.Orleans;

using Grpc.Core;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Persistent;

/// <summary>
///     Subscribe from KurrentDB persistent subscriptions and receive data from internal queue.
/// </summary>
/// <remarks>
///     Events are delivered in stream order only while one consumer is active on the consumer group. A
///     <c>MaxSubscriberCount</c> above one, or a consumer strategy that spreads events across consumers, lets several
///     receivers deliver parts of the same stream. Events are acknowledged as they leave the internal queue, which is how
///     persistent subscriptions are meant to work. Use the <c>$all</c> stream subscription when delivery must be
///     checkpointed only after events reach their consumers.
/// </remarks>
public class KurrentDBPersistentSubscriptionReceiver : IKurrentDBReceiver
{
    private readonly KurrentDBReceiverSettings _settings;
    private readonly KurrentDBPersistentSubscriptionReceiverOptions _receiverOptions;
    private readonly PersistentSubscriptionSettings _subscriptionSettings;
    private readonly UserCredentials? _credentials;
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<Uuid, bool> _hashSet = new();
    private readonly ConcurrentQueue<StreamedEvent<KurrentDBRawEvent>> _queue = new();

    // NOTE: this client is a keyed singleton owned by the container and shared by every queue of this connection.
    // It must never be disposed by this receiver.
    private readonly KurrentDBPersistentSubscriptionsClient _subscriptionClient;
    private PersistentSubscription? _subscription;
    private CancellationTokenSource? _cancellation;
    private readonly SemaphoreSlim _resubscribeLock = new(1, 1);
    // Written by init and close, read by the pulling agent and producer callers on other threads.
    private volatile bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBPersistentSubscriptionReceiver" />.
    /// </summary>
    /// <param name="subscriptionClient">The shared persistent subscriptions client for this connection.</param>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="logger">The logger.</param>
    /// <remarks>
    ///     The read position is kept by the KurrentDB server for the consumer group. The configured start position is
    ///     only used when the consumer group is created, or when it is updated with
    ///     <see cref="KurrentDBPersistentSubscriptionReceiverOptions.UpdateExistingSubscription" />.
    /// </remarks>
    public KurrentDBPersistentSubscriptionReceiver(KurrentDBPersistentSubscriptionsClient subscriptionClient, KurrentDBReceiverSettings settings, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(subscriptionClient, nameof(subscriptionClient));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(settings.Options, nameof(settings.Options));
        ArgumentNullException.ThrowIfNull(settings.ReceiverOptions, nameof(settings.ReceiverOptions));
        ArgumentException.ThrowIfNullOrEmpty(settings.ConsumerGroup, nameof(settings.ConsumerGroup));
        ArgumentException.ThrowIfNullOrEmpty(settings.QueueName, nameof(settings.QueueName));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        if (settings.ReceiverOptions is not KurrentDBPersistentSubscriptionReceiverOptions receiverOptions)
        {
            throw new ArgumentException($"{nameof(KurrentDBPersistentSubscriptionReceiver)} requires {nameof(KurrentDBPersistentSubscriptionReceiverOptions)}, but received {settings.ReceiverOptions.GetType().Name}.", nameof(settings));
        }
        ArgumentNullException.ThrowIfNull(receiverOptions.SubscriptionSettings, nameof(receiverOptions.SubscriptionSettings));
        _subscriptionClient = subscriptionClient;
        _settings = settings;
        _receiverOptions = receiverOptions;
        _credentials = settings.Options.Credentials?.ToUserCredentials();
        var origin = receiverOptions.SubscriptionSettings;

        // NOTE: settings.ReceiverOptions is the named options singleton shared by every queue of this stream provider.
        // Deriving a local copy keeps concurrently created receivers from clobbering each other's start position and
        // keeps the configured settings intact across reconnects.
        _subscriptionSettings = new PersistentSubscriptionSettings(origin.ResolveLinkTos, GetEventPosition(), origin.ExtraStatistics, origin.MessageTimeout, origin.MaxRetryCount, origin.LiveBufferSize, origin.ReadBatchSize, origin.HistoryBufferSize, origin.CheckPointAfter, origin.CheckPointLowerBound, origin.CheckPointUpperBound, origin.MaxSubscriberCount, origin.ConsumerStrategyName);
        _logger = logger;
        StreamPosition GetEventPosition()
        {
            if (receiverOptions.StartFromNow)
            {
                logger.PersistentStartingFromLatest(settings.Options.Name, settings.QueueName);
                return StreamPosition.End;
            }
            logger.PersistentStartingFromBeginning(settings.Options.Name, settings.QueueName);
            return StreamPosition.Start;
        }
    }

    /// <summary>
    ///     Start to create client and subscribe from KurrentDB persistent subscriptions.
    /// </summary>
    public async Task InitAsync()
    {
        var timer = Stopwatch.StartNew();
        try
        {
            _logger.PersistentReceiverInitializing(_settings.QueueName);
            // The subscriptions client is injected and shared; the consumer group is created if it does not exist.
            try
            {
                await _subscriptionClient.CreateToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, _subscriptionSettings, null, _credentials).ConfigureAwait(false);
            }
            catch (RpcException ex) when (ex.StatusCode is StatusCode.AlreadyExists)
            {
                // The consumer group already exists, which is the normal case on restart. Updating it would reset the
                // server side checkpoint and replay/skip events, so it is only done when explicitly requested.
                if (_receiverOptions.UpdateExistingSubscription)
                {
                    _logger.UpdatingSubscription(_settings.ConsumerGroup, _settings.QueueName);
                    await _subscriptionClient.UpdateToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, _subscriptionSettings, null, _credentials).ConfigureAwait(false);
                }
                else _logger.SubscriptionExists(_settings.ConsumerGroup, _settings.QueueName);
            }
            _cancellation = new CancellationTokenSource();
            var cancellationToken = _cancellation.Token;
            var subscription = await _subscriptionClient.SubscribeToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, OnEventAppeared, OnSubscriptionDropped, _credentials, _receiverOptions.PrefetchCount, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                subscription.Dispose();
                cancellationToken.ThrowIfCancellationRequested();
            }
            _subscription = subscription;
            _initialized = true;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.PersistentReceiverInitialized(_settings.QueueName, timer.Elapsed.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.PersistentReceiverInitializationFailed(ex, _settings.QueueName, timer.Elapsed.TotalMilliseconds);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to init KurrentDB persistent subscriptions client, {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <summary>
    ///     Clean up.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_initialized == false)
        {
            return;
        }
        try
        {
            // NOTE: only the subscription is disposed. The subscriptions client is a container owned keyed singleton
            // shared by every queue of this connection; disposing it here would break the other receivers.
            // Cancelling first stops any reconnect in progress from installing a new subscription.
            // Taking the reconnect lock waits for a cancelled reconnect to exit, so no subscription it creates leaks.
            _cancellation?.Cancel();
            await _resubscribeLock.WaitAsync().ConfigureAwait(false);
            try
            {
                _subscription?.Dispose();
            }
            finally
            {
                _resubscribeLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.PersistentReceiverCloseFailed(ex, _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to close KurrentDB persistent subscription, {ex.GetType()}: {ex.Message}"), ex);
        }
        finally
        {
            _cancellation?.Dispose();
            _cancellation = null;
            _subscription = null;
            _initialized = false;
        }
    }

    /// <summary>
    ///     Asking for more messages from internal queue.
    /// </summary>
    /// <param name="maxCount">Max amount of message which should be delivered in this request</param>
    /// <returns></returns>
    public List<StreamedEvent<KurrentDBRawEvent>> Receive(int maxCount)
    {
        _logger.PersistentReceivingEvents(_settings.QueueName);
        if (_initialized == false || _subscription == null)
        {
            return new List<StreamedEvent<KurrentDBRawEvent>>();
        }
        try
        {
            var streamedEvents = new List<StreamedEvent<KurrentDBRawEvent>>();
            var eventIds = new List<Uuid>();
            for (var i = 0; i < maxCount; i++)
            {
                if (!_queue.TryDequeue(out var streamedEvent))
                {
                    break;
                }
                streamedEvents.Add(streamedEvent);
                var eventId = Uuid.FromGuid(streamedEvent.EventId);
                eventIds.Add(eventId);
                _hashSet.TryRemove(eventId, out _);
            }
            if (eventIds.Count > 0)
            {
                _subscription.Ack(eventIds).Ignore();
            }
            return streamedEvents;
        }
        catch (Exception ex)
        {
            _logger.PersistentReceiveEventsFailed(ex, _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to receive events from internal queue for stream {_settings.QueueName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Persistent subscriptions track delivery on the server through the acknowledgements sent as events are
    ///     read, so there is no client side checkpoint to advance here.
    /// </remarks>
    public Task MessagesDeliveredAsync(ulong commitPosition)
    {
        return Task.CompletedTask;
    }

    #region Internal Queue Handlers

    private Task OnEventAppeared(PersistentSubscription subscription, ResolvedEvent resolvedEvent, int? retryCount, CancellationToken cancellationToken)
    {
        _logger.EventAppeared(subscription.SubscriptionId, _settings.QueueName);
        try
        {
            // Check duplication
            if (_hashSet.TryAdd(resolvedEvent.Event.EventId, true))
            {
                _queue.Enqueue(KurrentDBPersistentStreamedEventFactory.FromPersistentSubscription(resolvedEvent, _settings.Options.ConnectionName, _settings.StreamNameParser));
            }
        }
        catch (Exception ex)
        {
            _logger.HandleEventFailed(ex, _settings.QueueName);
        }
        return Task.CompletedTask;
    }

    private async void OnSubscriptionDropped(PersistentSubscription subscription, SubscriptionDroppedReason reason, Exception? exception)
    {
        try
        {
            switch (reason)
            {
                case SubscriptionDroppedReason.ServerError:
                    _logger.SubscriptionDroppedServerError(exception, subscription.SubscriptionId, _settings.QueueName);
                    await ResubscribeAsync(subscription).ConfigureAwait(false);
                    break;
                case SubscriptionDroppedReason.SubscriberError:
                    _logger.SubscriptionDroppedClientError(exception, subscription.SubscriptionId, _settings.QueueName);
                    await ResubscribeAsync(subscription).ConfigureAwait(false);
                    break;
                case SubscriptionDroppedReason.Disposed:
                    _logger.SubscriptionDisposed(subscription.SubscriptionId, _settings.QueueName);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.HandleSubscriptionDroppedFailed(ex, _settings.QueueName);
        }
    }

    /// <summary>
    ///     Replaces a dropped subscription, retrying with a capped exponential backoff until it succeeds or the
    ///     receiver is closed.
    /// </summary>
    private async Task ResubscribeAsync(PersistentSubscription dropped)
    {
        CancellationToken cancellationToken;
        try
        {
            var cancellation = _cancellation;
            if (_initialized == false || cancellation == null)
            {
                return;
            }
            cancellationToken = cancellation.Token;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        // Drops are queued rather than discarded, so a drop of a freshly installed subscription is never lost.
        try
        {
            await _resubscribeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        try
        {
            // The dropped subscription no longer delivers or acknowledges, so it is released.
            dropped.Dispose();

            // Only the current subscription is replaced; a stale drop must not tear down its replacement.
            if (cancellationToken.IsCancellationRequested || ReferenceEquals(_subscription, dropped) == false)
            {
                return;
            }
            _subscription = null;

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    var subscription = await _subscriptionClient.SubscribeToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, OnEventAppeared, OnSubscriptionDropped, _credentials, _receiverOptions.PrefetchCount, cancellationToken).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        subscription.Dispose();
                        return;
                    }
                    _subscription = subscription;
                    _logger.Reconnected(_settings.QueueName, attempt);
                    return;
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    var delay = KurrentDBReconnectPolicy.GetDelay(attempt, Random.Shared.NextDouble());
                    if (KurrentDBReconnectPolicy.GetLogLevel(attempt) is { } logLevel)
                    {
                        _logger.ReconnectFailed(logLevel, ex, _settings.QueueName, attempt, delay);
                    }
                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }
        finally
        {
            _resubscribeLock.Release();
        }
    }

    #endregion

}

internal static partial class KurrentDBPersistentSubscriptionReceiverLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read latest messages from KurrentDB queue {ConnectionName}-{QueueName}.")]
    public static partial void PersistentStartingFromLatest(this ILogger logger, string connectionName, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting to read messages from beginning of KurrentDB queue {ConnectionName}-{QueueName}.")]
    public static partial void PersistentStartingFromBeginning(this ILogger logger, string connectionName, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "KurrentDBPersistentSubscriptionReceiver for stream {QueueName} is initializing.")]
    public static partial void PersistentReceiverInitializing(this ILogger logger, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating existing persistent subscription {ConsumerGroup} on KurrentDB stream {QueueName}. This resets its start position.")]
    public static partial void UpdatingSubscription(this ILogger logger, string consumerGroup, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Persistent subscription {ConsumerGroup} already exists on KurrentDB stream {QueueName}; resuming from its existing position.")]
    public static partial void SubscriptionExists(this ILogger logger, string consumerGroup, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "InitAsync: QueueName={QueueName}, initialized in {ElapsedMilliseconds} ms")]
    public static partial void PersistentReceiverInitialized(this ILogger logger, string queueName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "InitAsync: QueueName={QueueName}, errored in {ElapsedMilliseconds} ms.")]
    public static partial void PersistentReceiverInitializationFailed(this ILogger logger, Exception exception, string queueName, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "CloseAsync: QueueName={QueueName}")]
    public static partial void PersistentReceiverCloseFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Receiving events from internal queue: {QueueName}")]
    public static partial void PersistentReceivingEvents(this ILogger logger, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to receive events from internal queue for stream {QueueName}.")]
    public static partial void PersistentReceiveEventsFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Trace, Message = "EventAppeared from subscription: {Subscription} for stream: {QueueName}")]
    public static partial void EventAppeared(this ILogger logger, string? subscription, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to handle event from subscription for stream {QueueName}.")]
    public static partial void HandleEventFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} with a server error.")]
    public static partial void SubscriptionDroppedServerError(this ILogger logger, Exception? exception, string? subscription, string queueName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} with a client error.")]
    public static partial void SubscriptionDroppedClientError(this ILogger logger, Exception? exception, string? subscription, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} successfully")]
    public static partial void SubscriptionDisposed(this ILogger logger, string? subscription, string queueName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to handle subscription dropped for stream {QueueName}.")]
    public static partial void HandleSubscriptionDroppedFailed(this ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Reconnected to the stream {QueueName} after {Attempts} attempt(s).")]
    public static partial void Reconnected(this ILogger logger, string queueName, int attempts);

    [LoggerMessage(Message = "Failed to reconnect to the stream {QueueName} on attempt {Attempt}. Retrying in {Delay}.")]
    public static partial void ReconnectFailed(this ILogger logger, LogLevel level, Exception exception, string queueName, int attempt, TimeSpan delay);
}
