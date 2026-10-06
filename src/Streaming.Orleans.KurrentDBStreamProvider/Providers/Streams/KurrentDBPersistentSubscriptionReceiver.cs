using System.Collections.Concurrent;
using System.Diagnostics;

using Continuum;
using Continuum.Streaming.Orleans;

using Grpc.Core;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Streaming.KurrentDBStorage;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Subscribe from KurrentDB persistent subscriptions and receive data from internal queue.
/// </summary>
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
    private bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBPersistentSubscriptionReceiver" />.
    /// </summary>
    /// <param name="subscriptionClient">The shared persistent subscriptions client for this connection.</param>
    /// <param name="settings">The per-queue receiver settings.</param>
    /// <param name="position">The last checkpointed position, if any.</param>
    /// <param name="logger">The logger.</param>
    public KurrentDBPersistentSubscriptionReceiver(KurrentDBPersistentSubscriptionsClient subscriptionClient, KurrentDBReceiverSettings settings, string position, ILogger logger)
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
            // If we have a position, read from position
            if (position.TryToStreamPosition(out var streamPosition))
            {
                logger.LogInformation("Starting to read from KurrentDB queue {0}-{1} at position {2}", settings.Options.Name, settings.QueueName, position);
            }
            // else, if configured to start from now, start reading from most recent 
            else if (receiverOptions.StartFromNow)
            {
                logger.LogInformation("Starting to read latest messages from KurrentDB queue {0}-{1}.", settings.Options.Name, settings.QueueName);
                streamPosition = StreamPosition.End;
            }
            // else, start reading from begining of the queue
            else
            {
                logger.LogInformation("Starting to read messages from begining of KurrentDB queue {0}-{1}.", settings.Options.Name, settings.QueueName);
                streamPosition = StreamPosition.Start;
            }
            return streamPosition;
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
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("KurrentDBPersistentSubscriptionReceiver for stream {QueueName} is initializing.", _settings.QueueName);
            }
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
                    _logger.LogInformation("Updating existing persistent subscription {ConsumerGroup} on KurrentDB stream {QueueName}. This resets its start position.", _settings.ConsumerGroup, _settings.QueueName);
                    await _subscriptionClient.UpdateToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, _subscriptionSettings, null, _credentials).ConfigureAwait(false);
                }
                else if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Persistent subscription {ConsumerGroup} already exists on KurrentDB stream {QueueName}; resuming from its existing position.", _settings.ConsumerGroup, _settings.QueueName);
                }
            }
            _subscription = await _subscriptionClient.SubscribeToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, OnEventAppeared, OnSubscriptionDropped, _credentials, _receiverOptions.PrefetchCount).ConfigureAwait(false);
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
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to init KurrentDB persistent subscriptions client, {ex.GetType()}: {ex.Message}"));
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
            _subscription?.Dispose();
            await Task.CompletedTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CloseAsync: QueueName={QueueName}", _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to close KurrentDB persistent subscription, {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
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
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Receiving events from internal queue: {QueueName}", _settings.QueueName);
        }
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
                // ToSequentialGuid is the inverse of the FromSequentialGuid used to build the carrier; ToGuid uses a
                // different byte order and would not recover the original event id.
                var eventId = Uuid.FromGuid(streamedEvent.EventId.ToSequentialGuid());
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
            _logger.LogError(ex, "Failed to receive events from internal queue for stream {QueueName}.", _settings.QueueName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to receive events from internal queue for stream {_settings.QueueName}. {ex.GetType()}: {ex.Message}"));
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
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("EventAppeared from subscription: {Subscription} for stream: {QueueName}", subscription.SubscriptionId, _settings.QueueName);
        }
        try
        {
            // Check duplication
            if (_hashSet.TryAdd(resolvedEvent.Event.EventId, true))
            {
                _queue.Enqueue(KurrentDBStreamedEventFactory.FromPersistentSubscription(resolvedEvent));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle event from subscription for stream {QueueName}.", _settings.QueueName);
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
                    _logger.LogWarning("SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} with server error: {Error}", subscription.SubscriptionId, _settings.QueueName, exception?.Message);
                    await ResubscribeAsync(1000);
                    break;
                case SubscriptionDroppedReason.SubscriberError:
                    _logger.LogWarning("SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} with client error: {Error}", subscription.SubscriptionId, _settings.QueueName, exception?.Message);
                    await ResubscribeAsync(10);
                    break;
                case SubscriptionDroppedReason.Disposed:
                    _logger.LogInformation("SubscriptionDropped from subscription: {Subscription} for stream: {QueueName} successfully", subscription.SubscriptionId, _settings.QueueName);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle subscription dropped for stream {QueueName}.", _settings.QueueName);
        }
    }

    private async Task ResubscribeAsync(int retryCount)
    {
        if (_initialized == false || _subscriptionClient == null)
        {
            return;
        }
        for (var i = 0; i < retryCount; i++)
        {
            try
            {
                _subscription = await _subscriptionClient.SubscribeToStreamAsync(_settings.QueueName, _settings.ConsumerGroup, OnEventAppeared, OnSubscriptionDropped, _credentials, _receiverOptions.PrefetchCount).ConfigureAwait(false);
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Successfully reconnected to the stream {QueueName}.", _settings.QueueName);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect to the stream {QueueName}. Retrying... Attempt: {Attempt}", _settings.QueueName, i + 1);
                await Task.Delay(TimeSpan.FromSeconds(i));
            }
        }
        _logger.LogError("Failed to reconnect to the stream {QueueName} after {Retries} attempts. Aborting...", _settings.QueueName, retryCount);
    }

    #endregion

}
