using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;

using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Continuum.Orleans.Events;

using Orleans.Storage;
using Continuum.Streams;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB.SelfHostedSubscriptions;

/// <summary>Identifies a catch-up subscription by name and implementing type.</summary>
public record CosmosDBCatchupSubscriptionInfo(string Name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)][property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type Type);

/// <summary>A subscription that reads the Cosmos DB change feed of a container written by the Cosmos DB log-consistency provider.</summary>
public abstract class CosmosDBCatchupSubscription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSubscription, TOptions> : IHostedStreamSubscription
    where TSubscription : CosmosDBCatchupSubscription<TSubscription, TOptions>
    where TOptions : CosmosDBCatchupSubscriptionOptions, new()
{
    /// <summary>The subscription name, derived from the type name without the Catchup/Subscription suffixes.</summary>
    public static readonly string Name;
    /// <summary>The implementing subscription type.</summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public static readonly Type Type = typeof(TSubscription);
    static CosmosDBCatchupSubscription()
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
    /// <summary>The name and type used to register this subscription.</summary>
    public static CosmosDBCatchupSubscriptionInfo Info => new(Name, Type);

    private readonly IServiceProvider _serviceProvider;

    /// <summary>The subscription logger.</summary>
    protected readonly ILogger<TSubscription> _logger;
    /// <summary>The subscription options.</summary>
    protected readonly TOptions _options;
    /// <summary>Deserializes event payloads.</summary>
    protected readonly IGrainStorageSerializer _grainStorageSerializer;
    /// <summary>Parses stream names into topic and key.</summary>
    protected readonly IStreamNameParser _streamNameParser;
    /// <summary>Resolves event types from mapped type names.</summary>
    protected readonly ITypeMapper _typeMapper;
    /// <summary>The Cosmos DB client for the configured connection.</summary>
    protected readonly CosmosClient _cosmosClient;

    private ChangeFeedProcessor? _changeFeedProcessor = null;
    private readonly CosmosDBCatchupSubscriptionStatusRegistry _statusRegistry;
    private readonly CosmosDBCatchupSubscriptionMetrics _metrics;
    private volatile CosmosDBCatchupSubscriptionState _state = CosmosDBCatchupSubscriptionState.Starting;
    private readonly HashSet<string> _failingLeases = [];
    private bool _infrastructureFailing;
    private readonly Lock _leaseLock = new();

    /// <summary>Creates the subscription, resolving its options and dependencies from the service provider.</summary>
    public CosmosDBCatchupSubscription(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TSubscription>>();
        _options = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>().Get(Name);
        _grainStorageSerializer = _options.GrainStorageSerializer;
        _streamNameParser = _options.StreamNameParser;
        _typeMapper = _options.TypeMapper;
        _cosmosClient = serviceProvider.GetRequiredKeyedService<CosmosClient>(_options.ConnectionName);
        _statusRegistry = serviceProvider.GetRequiredService<CosmosDBCatchupSubscriptionStatusRegistry>();
        _metrics = serviceProvider.GetRequiredService<CosmosDBCatchupSubscriptionMetrics>();
    }

    /// <inheritdoc/>
    public string SubscriptionName => Name;

    /// <summary>Starts the change feed processor unless the subscription is disabled.</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.SubscriptionDisabled(SubscriptionName);
            return;
        }

        // A second start must not orphan a running processor, which would keep holding its leases and processing.
        if (_changeFeedProcessor is { } previous)
        {
            _changeFeedProcessor = null;
            await previous.StopAsync().ConfigureAwait(false);
        }

        lock (_leaseLock)
        {
            _failingLeases.Clear();
            _infrastructureFailing = false;
        }

        SetState(CosmosDBCatchupSubscriptionState.Starting);
        _logger.SubscriptionStarting(SubscriptionName, _options.InstanceName);

        Container leaseContainer = _cosmosClient.GetContainer(_options.DatabaseName, _options.LeaseContainerName);
        Container monitoredContainer = _cosmosClient.GetContainer(_options.DatabaseName, _options.MonitoredContainerName);

        ChangeFeedProcessorBuilder builder = monitoredContainer
            .GetChangeFeedProcessorBuilder(SubscriptionName, HandleChangeStreamAsync)
            .WithErrorNotification(HandleErrorAsync)
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(leaseContainer);

        switch (_options.StartingPositionType)
        {
            case StartingPositionType.BeginningOfTime:
                builder.WithStartTime(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc));
                break;
            case StartingPositionType.FromDateTime:
                builder.WithStartTime(_options.StartingPositionDateTime!.Value.ToUniversalTime());
                break;
            case StartingPositionType.Latest:
                // Set explicitly rather than relying on the SDK default, which delivered existing changes.
                // Like any start time, it only applies when the leases are first created.
                builder.WithStartTime(DateTime.UtcNow);
                break;
        }

        builder
            .WithPollInterval(TimeSpan.FromMilliseconds(_options.PollingIntervalInMilliseconds))
            .WithMaxItems(_options.MaxItemsPerPoll);

        var processor = builder.Build();

        try
        {
            await processor.StartAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // A processor that failed to start is not kept, so a later stop does not act on it.
            SetState(CosmosDBCatchupSubscriptionState.Stopped, "The change feed processor failed to start.", ex);
            throw;
        }

        _changeFeedProcessor = processor;
        SetState(CosmosDBCatchupSubscriptionState.Running);

        _logger.SubscriptionStarted(SubscriptionName, _options.InstanceName);
    }

    /// <summary>Stops the change feed processor.</summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.SubscriptionStopping(SubscriptionName, _options.InstanceName);

        if (_changeFeedProcessor is { } processor)
        {
            _changeFeedProcessor = null;
            await processor.StopAsync().ConfigureAwait(false);
            SetState(CosmosDBCatchupSubscriptionState.Stopped);
        }

        _logger.SubscriptionStopped(SubscriptionName, _options.InstanceName);
    }

    internal async Task HandleChangeStreamAsync(ChangeFeedProcessorContext context, Stream changes, CancellationToken cancellationToken)
    {
        var response = await JsonSerializer.DeserializeAsync(changes, ChangeFeedJsonContext.Default.ChangeFeedResponse, cancellationToken).ConfigureAwait(false);
        await HandleChangesAsync(context, response?.Documents ?? [], cancellationToken).ConfigureAwait(false);
        // Several leases are processed in parallel, so the subscription only recovers once none of them is failing.
        // Checking the failing leases and updating the state happen under one lock so a concurrent failure cannot be overwritten.
        lock (_leaseLock)
        {
            var recovered = _failingLeases.Remove(context.LeaseToken) | _infrastructureFailing;
            // An infrastructure error may move the failing lease to another instance, so any successful batch clears it.
            _infrastructureFailing = false;
            if (recovered && _failingLeases.Count == 0 && _state == CosmosDBCatchupSubscriptionState.Retrying)
            {
                SetState(CosmosDBCatchupSubscriptionState.Running);
            }
        }
    }

    internal Task HandleErrorAsync(string leaseToken, Exception exception)
    {
        if (exception is ChangeFeedProcessorUserException userException)
        {
            // The change feed processor does not checkpoint the failed batch and delivers it again.
            _metrics.BatchRetried(SubscriptionName);
            lock (_leaseLock)
            {
                _failingLeases.Add(leaseToken);
                SetState(CosmosDBCatchupSubscriptionState.Retrying, $"Lease {leaseToken} failed to handle a batch.", userException.InnerException ?? userException);
            }
        }
        else
        {
            _metrics.Error(SubscriptionName);
            lock (_leaseLock)
            {
                _infrastructureFailing = true;
                SetState(CosmosDBCatchupSubscriptionState.Retrying, $"Lease {leaseToken} failed.", exception);
            }
        }
        return OnErrorAsync(leaseToken, exception);
    }

    private void SetState(CosmosDBCatchupSubscriptionState state, string? description = null, Exception? exception = null)
    {
        if (_state == state && description is null && exception is null && _statusRegistry.TryGetStatus(SubscriptionName, out _))
        {
            return;
        }
        _state = state;
        _statusRegistry.Report(new CosmosDBCatchupSubscriptionStatus(SubscriptionName, state, description, exception));
    }

    /// <summary>Converts a batch of changes to streamed events, skipping deleted, header, unknown and unreadable items.</summary>
    protected virtual async Task HandleChangesAsync(ChangeFeedProcessorContext context, 
        IReadOnlyCollection<ChangeFeedEventItem> changes, 
        CancellationToken cancellationToken)
    {
        var streamEvents = new List<IStreamedEvent<object>>(changes.Count);
        foreach (var change in changes)
        {
            if (change.Deleted)
            {
                continue;
            }
            if (change.Type != EventItemType.Event)
            {
                // Headers and snapshots are bookkeeping and derived state, not facts, so only events are delivered.
                continue;
            }
            if (!_typeMapper.TryGetType(change.DataType, out var type))
            {
                _logger.SkippingUnknownDataType(change.DataType, change.Id, change.StreamName);
                continue;
            }

            if (!CosmosDBCatchupEventDeserializer.TryDeserialize(change, type, _grainStorageSerializer, _logger, out var deserializedEvent))
            {
                continue;
            }
            IStreamedEvent<object> streamEvent;
            try
            {
                streamEvent = ToStreamEvent(context, change, deserializedEvent);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                // A malformed id or stream name is in the stored item itself, so redelivering it would fail forever and
                // block every other stream on the lease. The Continuum event sourcing provider never writes such items.
                _logger.SkippingMalformedChange(ex, change.Id, change.StreamName, context.LeaseToken, change.LogicalSequenceNumber);
                continue;
            }
            streamEvents.Add(streamEvent);
        }

        var started = Stopwatch.GetTimestamp();
        await HandleStreamEventsAsync(context, streamEvents, cancellationToken).ConfigureAwait(false);
        _metrics.BatchProcessed(SubscriptionName, streamEvents.Count, Stopwatch.GetElapsedTime(started));

        _logger.ChangesProcessed(changes.Count, context.Headers.RequestCharge);
    }

    /// <summary>Creates the streamed event for a deserialized change.</summary>
    protected virtual IStreamedEvent<object> ToStreamEvent(ChangeFeedProcessorContext context, ChangeFeedEventItem eventItem, object deserializedEvent)
    {
        DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds(eventItem.Timestamp).UtcDateTime;

        var streamEvent = new StreamedEvent<object>(
            eventId: Guid.Parse(eventItem.Id),
            eventType: eventItem.DataType,
            streamName: _streamNameParser.Parse(eventItem.StreamName),
            streamVersion: eventItem.Version,
            cursor: new StreamedEventCursor(_options.ConnectionName, _options.MonitoredContainerName, context.LeaseToken, eventItem.LogicalSequenceNumber, eventItem.SubSequenceNumber),
            timestamp: timestamp,
            @event: deserializedEvent,
            metadata: eventItem.Metadata is { Count: > 0 } metadata ? EventMetadata.Create(metadata!) : null);

        return streamEvent;
    }

    /// <summary>Handles the streamed events of a batch.</summary>
    protected abstract Task HandleStreamEventsAsync(ChangeFeedProcessorContext context,
        List<IStreamedEvent<object>> streamEvents,
        CancellationToken cancellationToken);

    /// <summary>Logs a change feed processor error.</summary>
    protected virtual Task OnErrorAsync(string leaseToken, Exception exception)
    {
        if (exception is ChangeFeedProcessorUserException userException)
        {
            _logger.LeaseUserDelegateFailed(userException.InnerException ?? userException, leaseToken);
        }
        else
        {
            _logger.LeaseFailed(exception, leaseToken);
        }

        return Task.CompletedTask;
    }
}

internal static partial class CosmosDBCatchupSubscriptionLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscription {SubscriptionName} is disabled.")]
    public static partial void SubscriptionDisabled(this ILogger logger, string subscriptionName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscription {SubscriptionName} for instance {InstanceName} is starting.")]
    public static partial void SubscriptionStarting(this ILogger logger, string subscriptionName, string instanceName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscription {SubscriptionName} for instance {InstanceName} has started.")]
    public static partial void SubscriptionStarted(this ILogger logger, string subscriptionName, string instanceName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscription {SubscriptionName} for instance {InstanceName} is stopping.")]
    public static partial void SubscriptionStopping(this ILogger logger, string subscriptionName, string instanceName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Subscription {SubscriptionName} for instance {InstanceName} has stopped.")]
    public static partial void SubscriptionStopped(this ILogger logger, string subscriptionName, string instanceName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipping change {Id} in stream {StreamName} with unknown data type {DataType}.")]
    public static partial void SkippingUnknownDataType(this ILogger logger, string dataType, string id, string streamName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Skipping change {Id} in stream {StreamName} on lease {LeaseToken} at logical sequence number {LogicalSequenceNumber}: its id or stream name is malformed.")]
    public static partial void SkippingMalformedChange(this ILogger logger, Exception exception, string id, string streamName, string leaseToken, long logicalSequenceNumber);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Processed {Count} changes using {RequestCharge} RUs.")]
    public static partial void ChangesProcessed(this ILogger logger, int count, double requestCharge);

    [LoggerMessage(Level = LogLevel.Error, Message = "Lease {LeaseToken} processing failed with an unhandled exception from the user delegate.")]
    public static partial void LeaseUserDelegateFailed(this ILogger logger, Exception exception, string leaseToken);

    [LoggerMessage(Level = LogLevel.Error, Message = "Lease {LeaseToken} failed.")]
    public static partial void LeaseFailed(this ILogger logger, Exception exception, string leaseToken);
}
