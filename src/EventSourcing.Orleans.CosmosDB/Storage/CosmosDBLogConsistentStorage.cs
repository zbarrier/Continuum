using System.Diagnostics;
using System.Net;

using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Orleans.Configuration;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     CosmosDB-based log consistent storage provider
/// </summary>
public class CosmosDBLogConsistentStorage : ILogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private const string ContentType = "application/json";

    private readonly ILogger<CosmosDBLogConsistentStorage> _logger;
    private readonly string _name;
    private readonly string _serviceId;
    private readonly CosmosDBLogConsistentStorageOptions _storageOptions;
    private readonly JsonSerializer _jsonSerializer;
    private readonly Func<string, string, GrainId, string> _streamNameFormatter;
    private readonly Func<NewId> _eventIdGenerator;
    private readonly ITypeMapper _typeMapper;

    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly Container _container;

    private bool _initialized = false;

    /// <summary>
    ///     Creates a new instance of the <see cref="CosmosDBLogConsistentStorage" /> type.
    /// </summary>
    public CosmosDBLogConsistentStorage(IServiceProvider serviceProvider,
        string name,
        CosmosDBLogConsistentStorageOptions storageOptions, 
        IOptions<ClusterOptions> clusterOptions, 
        ILogger<CosmosDBLogConsistentStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentNullException.ThrowIfNull(clusterOptions, nameof(clusterOptions));
        ArgumentNullException.ThrowIfNull(storageOptions, nameof(storageOptions));
        
        _logger = logger;
        _name = name;
        _serviceId = clusterOptions.Value.ServiceId;
        _storageOptions = storageOptions;
        _jsonSerializer = _storageOptions.JsonSerializer ?? JsonSerializer.CreateDefault();
        _streamNameFormatter = _storageOptions.StreamNameFormatter;
        _eventIdGenerator = _storageOptions.EventIdGenerator;
        _typeMapper = _storageOptions.TypeMapper;

        _client = serviceProvider.GetRequiredKeyedService<CosmosClient>(_storageOptions.ConnectionName);
        _database = _client.GetDatabase(_storageOptions.DatabaseName ?? throw new ArgumentNullException(nameof(_storageOptions.DatabaseName)));
        _container = _database.GetContainer(_storageOptions.ContainerName ?? throw new ArgumentNullException(nameof(_storageOptions.ContainerName)));
    }

    /// <summary>
    /// </summary>
    /// <param name="grainId"></param>
    /// <returns></returns>
    private string GetStreamName(GrainId grainId) => _streamNameFormatter(_serviceId, _name, grainId);

    #region Lifecycle Participant

    /// <inheritdoc />
    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<CosmosDBLogConsistentStorage>(_name);
        lifecycle.Subscribe(name, _storageOptions.InitStage, Init, Close);
    }

    private Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CosmosDBLogConsistentStorage {Name} is initializing: ServiceId={ServiceId}", _name, _serviceId);
            }

            _initialized = true;
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.LogDebug(
                    "Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms", 
                    _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, 
                "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms", 
                _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"));
        }
        return Task.CompletedTask;
    }

    private async Task Close(CancellationToken cancellationToken)
    {
        if (_initialized == false || _container is null)
        {
            return;
        }
        try
        {
            // We don't need to dispose the client because it's a singleton, could be used by
            // other providers and will be disposed by the container.
            //_client.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Close: Name={Name} ServiceId={ServiceId}", _name, _serviceId);
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"));
        }
    }

    #endregion

    #region Storage

    /// <inheritdoc />
    public async Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int toVersion)
    {
        List<TLogEntry> entries = new();

        if (_initialized == false || _container is null)
        {
            return entries;
        }

        double requestCharge = 0;
        var streamName = GetStreamName(grainId);
        
        try
        {
            var sql = new QueryDefinition("SELECT * FROM c WHERE c.type = 'evt' AND c.ver > @fromVer AND c.ver <= @toVer ORDER BY c.ver ASC")
                .WithParameter("@fromVer", fromVersion)
                .WithParameter("@toVer", fromVersion + toVersion);
            using (FeedIterator<EventItem> resultSetIterator = _container.GetItemQueryIterator<EventItem>(
                sql,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(streamName)
                }))
            {
                while (resultSetIterator.HasMoreResults)
                {
                    FeedResponse<EventItem> response = await resultSetIterator.ReadNextAsync().ConfigureAwait(false);
                    requestCharge += response.RequestCharge;
                    foreach (var item in response)
                    {
                        if (item is null)
                        {
                            _logger.LogWarning("Null event item in stream '{StreamName}'.", streamName);
                            continue;
                        }
                        var evtType = _typeMapper.GetType(item.DataType);
                        var evt = item.Data.ToObject(evtType, _jsonSerializer)!;
                        if (evt is TLogEntry logEntry)
                        {
                            entries.Add(logEntry);
                        }
                        else
                        {
                            _logger.LogWarning("Event type of '{EventType}' does not derive from the expected TLogEntry type of '{TLogEntry}' in stream '{StreamName}' with version '{StreamVersion}'.", 
                                evt.GetType(), typeof(TLogEntry), streamName, item.Version);
                        }
                    }
                }
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Reading '{EventCount}' events for stream '{StreamName}' consumed '{RequestUnits}' RUs", 
                    entries.Count, streamName, requestCharge);
            }

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
    {
        if (_initialized == false || _container is null)
        {
            return 0;
        }

        var streamName = GetStreamName(grainId);

        try
        {
            var result = await ReadHeaderItem(streamName).ConfigureAwait(false);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Reading the header item for stream '{StreamName}' consumed '{RequestUnits}' RUs", streamName, result.RequestCharge);
            }
            return result.EventItem is not null ? (int)result.EventItem.Version : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read last log entry for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read last log entry for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion)
    {
        if (_initialized == false || _container is null)
        {
            return 0;
        }

        var streamName = GetStreamName(grainId);

        if (entries.Count == 0)
        {
            return await GetLastVersionAsync(grainTypeName, grainId);
        }

        double requestCharge = 0;
        var transaction = _container.CreateTransactionalBatch(new PartitionKey(streamName));

        ulong currentVersion = (ulong)expectedVersion;

        if (currentVersion > 0)
        {
            var itemResponse = await ReadHeaderItem(streamName, throwOnNotFound: true, default).ConfigureAwait(false);
            requestCharge += itemResponse.RequestCharge;

            var headerItem = itemResponse.EventItem!;
            if (headerItem.Deleted)
            {
                throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Stream is deleted."));
            }
            if (headerItem.Version != currentVersion)
            {
                throw new InconsistentStateException($"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion}.");
            }

            headerItem.Version += (ulong)entries.Count;

            _ = transaction.ReplaceItem(headerItem.Id, headerItem, new TransactionalBatchItemRequestOptions
            {
                IfMatchEtag = headerItem.ETag,
            });
        }
        else
        {
            var header = new EventItem(ConvertToHeaderId(streamName), EventItemType.Header, streamName)
            {
                Version = (ulong)entries.Count,
                DataType = "StreamHeader.V1",
                Data = JToken.FromObject(new StreamHeader(false, false, new()), _jsonSerializer),
                Metadata = new()
            };
            _ = transaction.CreateItem(header);
        }

        ulong subSequenceNumber = 0;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        List<EventItem> eventItemsToAppend = new(entries.Count);

        foreach (var entry in entries)
        {
            if (entry is null)
            {
                throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Cannot append null entries."));
            }
            var eventDataEntry = new EventItem(_eventIdGenerator().ToSequentialGuid().ToString(), EventItemType.Event, streamName)
            {
                Version = ++currentVersion,
                SubSequenceNumber = subSequenceNumber++,
                DataType = _typeMapper.GetTypeName(entry.GetType()),
                Data = JToken.FromObject(entry, _jsonSerializer),
                Metadata = new()
            };
            eventItemsToAppend.Add(eventDataEntry);
        }

        var firstBatchSize = _storageOptions.BatchSize - 1;
        var firstBatch = eventItemsToAppend.Take(firstBatchSize);
        foreach (var item in firstBatch)
        {
            _ = transaction.CreateItem(item);
        }

        var response = await transaction.ExecuteAsync().ConfigureAwait(false);
        requestCharge += response.RequestCharge;

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Stream already exists. RequestCharge: {requestCharge}"));
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. RequestCharge: {requestCharge}, StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}"));
        }

        if (entries.Count > firstBatchSize)
        {
            // We have more than one batch to write (the first batch was already written above)
            // so we need to write the remaining batches in a separate transaction.
            // (CosmosDB only supports 100 operations per transaction)
            var remainingItemBatches = eventItemsToAppend.Skip(firstBatchSize).Batch((uint)_storageOptions.BatchSize);
            foreach (var batch in remainingItemBatches)
            {
                if (batch.Count() == 0)
                {
                    continue;
                }

                transaction = _container.CreateTransactionalBatch(new PartitionKey(streamName));

                foreach (var item in batch)
                {
                    _ = transaction.CreateItem(item);
                }

                response = await transaction.ExecuteAsync().ConfigureAwait(false);
                requestCharge += response.RequestCharge;

                if (!response.IsSuccessStatusCode)
                {
                    throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. RequestCharge: {requestCharge}, StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}"));
                }
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Writing '{EventCount}' events to stream '{StreamName}' consumed '{RequestUnits}' RUs", 
                eventItemsToAppend.Count + 1, streamName, requestCharge);
        }

        return (int)currentVersion;
    }

    private async Task<EventItemResponse> ReadHeaderItem(string streamName, bool throwOnNotFound = false, CancellationToken cancellationToken = default)
    {
        string headerId = ConvertToHeaderId(streamName);
        using (var responseMessage = await _container.ReadItemStreamAsync(headerId, new PartitionKey(streamName)).ConfigureAwait(false))
        {
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                if (throwOnNotFound)
                {
                    throw new InvalidOperationException($"Header item not found for stream '{streamName}'.");
                }
                return new EventItemResponse(null, responseMessage.Headers.RequestCharge);
            }
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to read the header item for stream '{streamName}'. Reason: {responseMessage.ErrorMessage}");
            }
            var item = FromStream<EventItem>(responseMessage.Content) ?? throw new InvalidOperationException($"Found header item for stream '{streamName}' but failed to deserialize the EventDataItem.");
            return new EventItemResponse(item, responseMessage.Headers.RequestCharge);
        }
    }

    #endregion

    #region Serialize & Deserialize

    private string ConvertToHeaderId(string streamName)
    {
        return streamName.Replace("/", "_");
    }

    private StreamHeader DeserializeHeader(EventItem item)
    {
        var header = item.Data.ToObject<StreamHeader>(_jsonSerializer) ?? throw new InvalidOperationException($"Failed to deserialize the header item for stream '{item.StreamName}'.");
        return header;
    }

    private T? FromStream<T>(System.IO.Stream stream)
    {
        using (stream)
        {
            if (typeof(System.IO.Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)(stream);
            }
            using (var sr = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return _jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }
    }

    #endregion
}