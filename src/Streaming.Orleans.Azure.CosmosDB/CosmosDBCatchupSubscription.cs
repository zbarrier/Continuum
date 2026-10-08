using System.Numerics;

using Continuum.Streaming.Orleans.Azure.CosmosDB.Configuration;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Continuum.Streaming.Orleans.Azure.CosmosDB;

public record CosmosDBCatchupSubscriptionInfo(string Name, Type Type);

public abstract class CosmosDBCatchupSubscription<TSubscription, TOptions> : IHostedStreamSubscription
    where TSubscription : CosmosDBCatchupSubscription<TSubscription, TOptions>
    where TOptions : CosmosDBCatchupSubscriptionOptions, new()
{
    public static readonly string Name;
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
    public static CosmosDBCatchupSubscriptionInfo Info => new(Name, Type);

    private readonly IServiceProvider _serviceProvider;

    protected readonly ILogger<TSubscription> _logger;
    protected readonly TOptions _options;
    protected readonly JsonSerializer _jsonSerializer;
    protected readonly IStreamedNameParser _streamNameParser;
    protected readonly ITypeMapper _typeMapper;
    protected readonly CosmosClient _cosmosClient;

    private ChangeFeedProcessor? _changeFeedProcessor = null;

    public CosmosDBCatchupSubscription(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TSubscription>>();
        _options = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>().Get(Name);
        _jsonSerializer = _options.JsonSerializer;
        _streamNameParser = _options.StreamNameParser;
        _typeMapper = _options.TypeMapper;
        _cosmosClient = serviceProvider.GetRequiredKeyedService<CosmosClient>(_options.ConnectionName);
    }

    public string SubscriptionName => Name;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Subscription {SubscriptionName} is disabled.", SubscriptionName);
            return;
        }

        _logger.LogWarning("Subscription {SubscriptionName} for instance {InstanceName} is starting.", SubscriptionName, _options.InstanceName);

        Container leaseContainer = _cosmosClient.GetContainer(_options.DatabaseName, _options.LeaseContainerName);
        Container monitoredContainer = _cosmosClient.GetContainer(_options.DatabaseName, _options.MonitoredContainerName);

        ChangeFeedProcessorBuilder builder = monitoredContainer
            .GetChangeFeedProcessorBuilder<ChangeFeedEventItem>(SubscriptionName, HandleChangesAsync)
            .WithErrorNotification(OnErrorAsync)
            .WithInstanceName(_options.InstanceName)
            .WithLeaseContainer(leaseContainer);

        switch (_options.StartingPositionType)
        {
            case StartingPositionType.BeginningOfTime:
                builder.WithStartTime(DateTime.MinValue.ToUniversalTime());
                break;
            case StartingPositionType.FromDateTime:
                builder.WithStartTime(_options.StartingPositionDateTime!.Value.ToUniversalTime());
                break;
            default:
                // Do nothing
                break;
        }   

        builder
            .WithPollInterval(TimeSpan.FromMilliseconds(_options.PollingIntervalInMilliseconds))
            .WithMaxItems(_options.MaxItemsPerPoll);

        _changeFeedProcessor = builder.Build();

        await _changeFeedProcessor.StartAsync().ConfigureAwait(false);

        _logger.LogWarning("Subscription {SubscriptionName} for instance {InstanceName} has started.", SubscriptionName, _options.InstanceName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning("Subscription {SubscriptionName} for instance {InstanceName} is stopping.", SubscriptionName, _options.InstanceName);

        if (_changeFeedProcessor is not null)
        {
            await _changeFeedProcessor.StopAsync().ConfigureAwait(false);
        }

        _logger.LogWarning("Subscription {SubscriptionName} for instance {InstanceName} has stopped.", SubscriptionName, _options.InstanceName);
    }

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
            if (change.Type == EventItemType.Header && _options.IgnoreHeaderEvents)
            {
                continue;
            }
            if (!_typeMapper.TryGetType(change.DataType, out var type))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Skipping change with unknown data type {DataType}.", change.DataType);
                }
                continue;
            }

            if (!CosmosDBCatchupEventDeserializer.TryDeserialize(change, type, _jsonSerializer, _logger, out var deserializedEvent))
            {
                continue;
            }
            var streamEvent = ToStreamEvent(context, change, deserializedEvent);
            streamEvents.Add(streamEvent);
        }

        await HandleStreamEventsAsync(context, streamEvents, cancellationToken).ConfigureAwait(false);

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Processed {Count} changes using {RequestCharge} RUs.", changes.Count, context.Headers.RequestCharge);
        }
    }

    protected virtual IStreamedEvent<object> ToStreamEvent(ChangeFeedProcessorContext context, ChangeFeedEventItem eventItem, object deserializedEvent)
    {
        string streamKey = _streamNameParser.GetStreamKey(eventItem.StreamName);
        DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds(eventItem.Timestamp).UtcDateTime;

        var streamEvent = new StreamedEvent<object>(
            eventId: NewId.FromSequentialGuid(Guid.Parse(eventItem.Id)),
            eventType: eventItem.DataType,
            streamName: eventItem.StreamName,
            streamKey: streamKey,
            topic: _options.MonitoredContainerName,
            partitionId: context.LeaseToken,
            streamVersion: eventItem.Version,
            streamPosition: eventItem.Version,
            sequenceNumber: eventItem.LogicalSequenceNumber,
            subSequenceNumber: eventItem.SubSequenceNumber,
            timestamp: timestamp,
            @event: deserializedEvent);

        return streamEvent;
    }

    protected abstract Task HandleStreamEventsAsync(ChangeFeedProcessorContext context,
        List<IStreamedEvent<object>> streamEvents,
        CancellationToken cancellationToken);

    protected virtual Task OnErrorAsync(string leaseToken, Exception exception)
    {
        if (exception is ChangeFeedProcessorUserException userException)
        {
            _logger.LogError("Lease {LeaseToken} processing failed with unhandled exception from user delegate {UserExceptionInnerException}",
                leaseToken, userException.InnerException);
        }
        else
        {
            _logger.LogError("Lease {LeaseToken} failed with {Exception}", leaseToken, exception);
        }

        return Task.CompletedTask;
    }
}
