using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

using Continuum.Streaming.Orleans.KurrentDB.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Continuum.Serialization.Orleans;

using Orleans.Storage;

using Continuum.Streaming.Orleans;

namespace Continuum.Streaming.Orleans.KurrentDB;

/// <summary>
///     Identifies a catch-up subscription for registration.
/// </summary>
/// <param name="Name">The subscription name, which is also its options name.</param>
/// <param name="Type">The subscription type.</param>
public record KurrentDBCatchupSubscriptionInfo(string Name, Type Type);

/// <summary>
///     A hosted catch-up subscription that reads a KurrentDB stream and delivers events in batches.
/// </summary>
/// <typeparam name="TSubscription">The concrete subscription type.</typeparam>
/// <typeparam name="TOptions">The subscription options type.</typeparam>
public abstract class KurrentDBCatchupSubscription<TSubscription, TOptions> : BackgroundService, IStreamSubscription
    where TSubscription : KurrentDBCatchupSubscription<TSubscription, TOptions>
    where TOptions : KurrentDBCatchupSubscriptionOptions, new()
{
    /// <summary>
    ///     The subscription name, derived from <typeparamref name="TSubscription" /> without a <c>CatchupSubscription</c> suffix.
    /// </summary>
    public static readonly string Name;

    /// <summary>
    ///     The subscription type.
    /// </summary>
    public static readonly Type Type = typeof(TSubscription);
    static KurrentDBCatchupSubscription()
    {
        string name = typeof(TSubscription).Name;
        if (name.EndsWith("Subscription"))
        {
            name = name[..^12];
        }
        if (name.EndsWith("Catchup"))
        {
            name = name[..^7];
        }
        Name = name;
    }
    /// <summary>
    ///     The registration info for this subscription.
    /// </summary>
    public static KurrentDBCatchupSubscriptionInfo Info => new(Name, Type);

    private readonly IServiceProvider _serviceProvider;

    /// <summary>The logger.</summary>
    protected readonly ILogger<TSubscription> _logger;
    /// <summary>The subscription options.</summary>
    protected readonly TOptions _options;
    /// <summary>The KurrentDB client the subscription reads from.</summary>
    protected readonly KurrentDBClient _client;
    /// <summary>The monitor that commits the subscription's checkpoint.</summary>
    protected readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    /// <summary>The serializer used to deserialize event payloads.</summary>
    protected readonly IGrainStorageSerializer _storageSerializer;
    /// <summary>The type mapper used to resolve event types.</summary>
    protected readonly ITypeMapper _typeMapper;

    /// <summary>The channel buffering events between the subscription and the batch processor.</summary>
    protected readonly Channel<StreamedEvent<object>> _channel;

    /// <summary>The task processing events from <see cref="_channel" />.</summary>
    protected Task? _receivingTask = null;


    /// <summary>
    ///     Initializes a new instance of the subscription.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve options and dependencies.</param>
    public KurrentDBCatchupSubscription(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TSubscription>>();
        _options = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>().Get(Name);
        _client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_options.ConnectionName);
        _checkpointMonitor = new KurrentDBCheckpointMonitor(_logger, _options.CheckpointMonitorOptions, _options.CheckpointStore,
            SubscriptionName, StreamName);
        _storageSerializer = _options.GrainStorageSerializer;
        _typeMapper = _options.TypeMapper;

        _channel = Channel.CreateBounded<StreamedEvent<object>>(new BoundedChannelOptions(_options.BufferOptions.MaxUnprocessedEventsBeforeStop)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,

            // This is the default value, but being explicit for clarity.  This allows the writer to continue writing
            // to the channel without being blocked by a slow reader, while still ensuring that the reader processes
            // events in a timely manner.
            AllowSynchronousContinuations = false
        });
    }

    /// <summary>
    ///     The subscription name.
    /// </summary>
    public string SubscriptionName => Name;

    /// <summary>
    ///     The stream the subscription reads.
    /// </summary>
    public abstract StreamName StreamName { get; }

    /// <summary>
    ///     The server-side filter applied when reading <c>$all</c>, or <see langword="null" /> for none.
    /// </summary>
    public abstract IEventFilter? FilterOptions { get; }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (StreamName.IsAllStream)
        {
            _receivingTask = ReceiveFromAllStreamChannel();
            await SubscribeToAll(cancellationToken);
        }
        else
        {
            _receivingTask = ReceiveFromStreamChannel();
            await SubscribeToStream(cancellationToken);
        }
    }

    #region Subscribe To All

    private async Task SubscribeToAll(CancellationToken cancellationToken)
    {
        Subscribe:
        try
        {
            var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync();
            var startPosition = !lastCheckpoint.HasValue ? FromAll.Start : FromAll.After(new Position(lastCheckpoint.Value, lastCheckpoint.Value));
            var subscriptionFilterOptions = FilterOptions != null 
                ? new SubscriptionFilterOptions(FilterOptions, _options.CheckpointMonitorOptions.KurrentDBCheckpointInterval)
                : null;

            await using var subscription = _client.SubscribeToAll(startPosition, false, subscriptionFilterOptions, null, cancellationToken);

            await foreach (var message in subscription.Messages)
            {
                switch (message)
                {
                    case StreamMessage.Event(ResolvedEvent e):
                        await EventAppearedFromAllStream(e, cancellationToken);
                        break;

                    case StreamMessage.AllStreamCheckpointReached(Position position):
                        {
                            _logger.LogInformation("Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.", position, SubscriptionName);
                            var checkpointEvent = position.ToAllStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        _logger.LogInformation("Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.", SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.LogInformation("Subscription '{SubscriptionName}' has caught up to the head of the stream.", SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.LogWarning("Subscription '{SubscriptionName}' has fallen behind. This means the subscription is processing events slower than they are being produced. Consider scaling up the processing or increasing the buffer size.", SubscriptionName);
                        break;

                    case StreamMessage.LastAllStreamPosition(var position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached last position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToAllStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.NotFound:
                        _logger.LogError("Subscription '{SubscriptionName}' was not found. This likely indicates a misconfiguration or an issue with the KurrentDB client. Restarting subscription.", SubscriptionName);
                        break;

                    case StreamMessage.Unknown:
                        _logger.LogError("Received unknown message type in subscription '{SubscriptionName}'.", SubscriptionName);
                        break;

                    default:
                        _logger.LogWarning("Received unhandled message type '{MessageType}' in subscription '{SubscriptionName}'.", message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled by the user.", SubscriptionName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Subscription '{SubscriptionName}' was dropped.  Restarting subscription.", SubscriptionName);
            goto Subscribe;
        }
    }

    private async Task EventAppearedFromAllStream(ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        var eventHasBeenDeleted = resolvedEvent.Event is null;
        if (eventHasBeenDeleted)
        {
            return;
        }
        var eventTypeNotMapped = !_typeMapper.TryGetType(resolvedEvent.Event!.EventType, out var eventType);
        if (eventTypeNotMapped)
        {
            _logger.LogWarning("Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'.",
                resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToAllStreamEvent(deserializedEvent);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    private async Task ReceiveFromAllStreamChannel()
    {
        ulong lastStreamPosition = 0;
        var eventBatch = new List<IStreamedEvent<object>>();
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var streamedEvent))
            {
                lastStreamPosition = StreamSequenceNumber.ToCommitPosition(streamedEvent.SequenceNumber, streamedEvent.StreamName);

                if (ReferenceEquals(streamedEvent.Event, StreamEventExtensions.CheckpointMarker))
                {
                    continue;
                }

                eventBatch.Add(streamedEvent);

                bool isBatchFull = eventBatch.Count >= _options.BufferOptions.MaxUnprocessedEventsBeforeStop;
                if (isBatchFull)
                {
                    // If the batch is full, we want to check if the next event is from a different transaction/stream.
                    // If it is, we want to process the current batch before reading more events.
                    // If a transaction spans multiple streams, it could still get split between batches.
                    bool isDifferentStream = isBatchFull && !streamedEvent.StreamName.Equals(eventBatch[^1].StreamName);
                    if (isDifferentStream)
                    {
                        break;
                    }
                }
            }

            if (eventBatch.Count > 0)
            {
                await OnNextBatchAsync(eventBatch);
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition);
                eventBatch.Clear();
            }
            else if (lastStreamPosition > 0)
            {
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition);
            }
        }
    }

    #endregion

    #region Subscribe To Stream

    private async Task SubscribeToStream(CancellationToken cancellationToken)
    {
        Subscribe:
        try
        {
            var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync();
            var startPosition = !lastCheckpoint.HasValue ? FromStream.Start : FromStream.After(new StreamPosition(lastCheckpoint.Value));

            await using var subscription = _client.SubscribeToStream(StreamName, startPosition, false, null, cancellationToken);

            await foreach (var message in subscription.Messages)
            {
                switch (message)
                {
                    case StreamMessage.Event(var e):
                        await EventAppearedFromStream(e, cancellationToken);
                        break;

                    case StreamMessage.StreamCheckpointReached(StreamPosition position):
                        {
                            _logger.LogInformation("Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.", position, SubscriptionName);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        _logger.LogInformation("Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.", SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.LogInformation("Subscription '{SubscriptionName}' has caught up to the head of the stream.", SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.LogWarning("Subscription '{SubscriptionName}' has fallen behind. This means the subscription is processing events slower than they are being produced. Consider scaling up the processing or increasing the buffer size.", SubscriptionName);
                        break;

                    case StreamMessage.FirstStreamPosition(StreamPosition position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached first position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.LastStreamPosition(StreamPosition position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached last position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.NotFound:
                        _logger.LogError("Subscription '{SubscriptionName}' was not found. This likely indicates a misconfiguration or an issue with the KurrentDB client. Restarting subscription.", SubscriptionName);
                        break;

                    case StreamMessage.Unknown:
                        _logger.LogWarning("Received unknown message type in subscription '{SubscriptionName}'.", SubscriptionName);
                        break;

                    default:
                        _logger.LogWarning("Received unhandled message type '{MessageType}' in subscription '{SubscriptionName}'.", message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled by the user.", SubscriptionName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Subscription '{SubscriptionName}' was dropped.  Restarting subscription.", SubscriptionName);
            goto Subscribe;
        }
    }

    private async Task EventAppearedFromStream(ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        var eventHasBeenDeleted = resolvedEvent.Event is null;
        if (eventHasBeenDeleted)
        {
            return;
        }
        var eventTypeNotMapped = !_typeMapper.TryGetType(resolvedEvent.Event!.EventType, out var eventType);
        if (eventTypeNotMapped)
        {
            _logger.LogWarning("Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'.",
                resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToStreamEvent(deserializedEvent);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    private bool TryDeserialize(ResolvedEvent resolvedEvent, Type eventType, [NotNullWhen(true)] out object? deserializedEvent)
    {
        var record = resolvedEvent.Event;
        return KurrentDBCatchupEventDeserializer.TryDeserialize(_storageSerializer, _logger, record.EventType, record.EventStreamId, record.Data, eventType, SubscriptionName, out deserializedEvent);
    }

    private async Task ReceiveFromStreamChannel()
    {
        ulong lastStreamPosition = 0;
        var eventBatch = new List<IStreamedEvent<object>>();
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var streamEvent))
            {
                lastStreamPosition = streamEvent.StreamPosition;

                if (ReferenceEquals(streamEvent.Event, StreamEventExtensions.CheckpointMarker))
                {
                    continue;
                }

                eventBatch.Add(streamEvent);

                bool isBatchFull = eventBatch.Count >= _options.BufferOptions.MaxUnprocessedEventsBeforeStop;
                if (isBatchFull)
                {
                    // If the batch is full, we want to check if the next event is from a different transaction/stream.
                    // If it is, we want to process the current batch before reading more events.
                    // If a transaction spans multiple streams, it could still get split between batches.
                    bool isDifferentStream = isBatchFull && !streamEvent.StreamName.Equals(eventBatch[^1].StreamName);
                    if (isDifferentStream)
                    {
                        break;
                    }
                }
            }

            if (eventBatch.Count > 0)
            {
                await OnNextBatchAsync(eventBatch);
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition);
                eventBatch.Clear();
            }
            else if (lastStreamPosition > 0)
            {
                await _checkpointMonitor.TryCheckpointAsync(lastStreamPosition);
            }
        }
    }

    #endregion

    /// <summary>
    ///     Processes a batch of events read from the stream.
    /// </summary>
    /// <param name="streamEvents">The events, in stream order.</param>
    protected abstract Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents);
}
