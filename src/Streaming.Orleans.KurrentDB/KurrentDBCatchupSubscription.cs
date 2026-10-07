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

public record KurrentDBCatchupSubscriptionInfo(string Name, Type Type);

public abstract class KurrentDBCatchupSubscription<TSubscription, TOptions> : BackgroundService, IStreamSubscription
    where TSubscription : KurrentDBCatchupSubscription<TSubscription, TOptions>
    where TOptions : KurrentDBCatchupSubscriptionOptions, new()
{
    public static readonly string Name;
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
    public static KurrentDBCatchupSubscriptionInfo Info => new(Name, Type);

    private readonly IServiceProvider _serviceProvider;

    protected readonly ILogger<TSubscription> _logger;
    protected readonly TOptions _options;
    protected readonly KurrentDBClient _client;
    protected readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    protected readonly IGrainStorageSerializer _storageSerializer;
    protected readonly ITypeMapper _typeMapper;

    protected readonly Channel<StreamedEvent<object>> _channel;

    protected Task? _receivingTask = null;


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

    public string SubscriptionName => Name;
    public abstract StreamName StreamName { get; }
    public abstract IEventFilter? FilterOptions { get; }

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

                if (streamedEvent.Event is null)
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

    /// <summary>
    /// Deserializes the event, logging and skipping it when it has no payload, cannot be read or reads as null, so that
    /// a bad event written by a producer outside our control does not halt the subscription.
    /// </summary>
    private bool TryDeserialize(ResolvedEvent resolvedEvent, Type eventType, [NotNullWhen(true)] out object? deserializedEvent)
    {
        deserializedEvent = null;
        var record = resolvedEvent.Event;
        if (record.Data.IsEmpty)
        {
            _logger.LogWarning("Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it has no payload.",
                record.EventType, record.EventStreamId, SubscriptionName);
            return false;
        }
        try
        {
            deserializedEvent = _storageSerializer.Deserialize<object>(new BinaryDataWithType(record.Data, eventType));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it could not be deserialized.",
                record.EventType, record.EventStreamId, SubscriptionName);
            return false;
        }
        if (deserializedEvent is null)
        {
            _logger.LogWarning("Skipping event of type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}' because it deserialized to null.",
                record.EventType, record.EventStreamId, SubscriptionName);
            return false;
        }
        return true;
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

                if (streamEvent.Event is null)
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

    protected abstract Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents);
}