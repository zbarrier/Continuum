using System.Diagnostics;
using System.Net;
using System.Text.Json;

using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.CosmosDB;

/// <summary>
///     CosmosDB-based log consistent storage provider
/// </summary>
public class CosmosDBLogConsistentStorage : ILogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private const string HeaderId = "hdr";
    private const string HeaderDataType = "StreamHeader.V1";
    private const string PartitionKeyPath = "/streamName";

    private readonly ILogger<CosmosDBLogConsistentStorage> _logger;
    private readonly string _name;
    private readonly string _serviceId;
    private readonly CosmosDBLogConsistentStorageOptions _storageOptions;
    private readonly IGrainStorageSerializer _storageSerializer;
    private readonly Func<string, string, GrainId, string> _streamNameFormatter;
    private readonly Func<NewId> _eventIdGenerator;
    private readonly ITypeMapper _typeMapper;

    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly Container _container;
    private readonly LogConsistentStorageInstrumentation _instrumentation;

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
        _storageSerializer = _storageOptions.GrainStorageSerializer;
        _streamNameFormatter = _storageOptions.StreamNameFormatter;
        _eventIdGenerator = _storageOptions.EventIdGenerator;
        _typeMapper = _storageOptions.TypeMapper;

        _client = serviceProvider.GetRequiredKeyedService<CosmosClient>(_storageOptions.ConnectionName);
        _database = _client.GetDatabase(_storageOptions.DatabaseName);
        _container = _database.GetContainer(_storageOptions.ContainerName);
        _instrumentation = new LogConsistentStorageInstrumentation(serviceProvider, "azure.cosmosdb", name, recordsRequestCharge: true);
    }

    /// <summary>
    ///     Gets the stream name, which is also the partition key value, for a grain.
    /// </summary>
    /// <param name="grainId">The grain whose stream name is returned.</param>
    /// <returns>The stream name produced by <see cref="CosmosDBLogConsistentStorageOptions.StreamNameFormatter"/>.</returns>
    private string GetStreamName(GrainId grainId) => _streamNameFormatter(_serviceId, _name, grainId);

    #region Lifecycle Participant

    /// <inheritdoc />
    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<CosmosDBLogConsistentStorage>(_name);
        lifecycle.Subscribe(name, _storageOptions.InitStage, Init, Close);
    }

    private async Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("CosmosDBLogConsistentStorage {Name} is initializing: ServiceId={ServiceId}", _name, _serviceId);
            }

            await ValidateContainerAsync(cancellationToken);

            _initialized = true;
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                timer.Stop();
                _logger.LogDebug(
                    "Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms", 
                    _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            }
        }
        catch (CosmosDBLogConsistentStorageException ex)
        {
            timer.Stop();
            _logger.LogError(ex,
                "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms",
                _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            throw;
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, 
                "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms", 
                _name, _serviceId, timer.Elapsed.TotalMilliseconds.ToString("0.00"));
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"), ex);
        }
    }

    // The provider does not create resources; they are expected to be provisioned beforehand (e.g. by infrastructure as code).
    // Fail fast at startup with a clear message instead of failing on the first read or write.
    private async Task ValidateContainerAsync(CancellationToken cancellationToken)
    {
        var properties = await ReadContainerWithRetryAsync(cancellationToken);

        var paths = properties.PartitionKeyPaths ?? (properties.PartitionKeyPath is null ? [] : [properties.PartitionKeyPath]);
        if (paths.Count != 1 || !string.Equals(paths[0], PartitionKeyPath, StringComparison.Ordinal))
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant(
                $"Cosmos DB container '{_storageOptions.ContainerName}' in database '{_storageOptions.DatabaseName}' has partition key '{string.Join(", ", paths)}', but the provider requires '{PartitionKeyPath}'."));
        }
    }

    // Connectivity failures are transient, so they are retried until StartupConnectionTimeout elapses. Configuration
    // failures (missing container, authorization) are permanent and fail immediately.
    private async Task<ContainerProperties> ReadContainerWithRetryAsync(CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        var delay = TimeSpan.FromSeconds(1);
        var attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                return (await _container.ReadContainerAsync(cancellationToken: cancellationToken)).Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant(
                    $"Cosmos DB container '{_storageOptions.ContainerName}' in database '{_storageOptions.DatabaseName}' was not found. The provider does not create resources; create the container with partition key path '{PartitionKeyPath}' before starting the silo."), ex);
            }
            catch (Exception ex) when (IsTransient(ex, cancellationToken))
            {
                var remaining = _storageOptions.StartupConnectionTimeout - Stopwatch.GetElapsedTime(startTimestamp);
                if (remaining <= TimeSpan.Zero)
                {
                    throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant(
                        $"Could not connect to Cosmos DB to validate container '{_storageOptions.ContainerName}' in database '{_storageOptions.DatabaseName}' within {_storageOptions.StartupConnectionTimeout.TotalSeconds:0} seconds ({attempt} attempts)."), ex);
                }

                _logger.LogWarning(ex,
                    "Init: Name={Name} ServiceId={ServiceId}, Cosmos DB is unreachable (attempt {Attempt}), retrying in {Delay} ms",
                    _name, _serviceId, attempt, delay.TotalMilliseconds);

                await Task.Delay(delay < remaining ? delay : remaining, cancellationToken);
                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, TimeSpan.FromSeconds(8).Ticks));
            }
        }
    }

    private static bool IsTransient(Exception ex, CancellationToken cancellationToken) => ex switch
    {
        HttpRequestException => true,
        CosmosException cosmos => cosmos.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError or HttpStatusCode.GatewayTimeout,
        // SDK request timeouts surface as OperationCanceledException; silo shutdown cancellation is not retried.
        OperationCanceledException => !cancellationToken.IsCancellationRequested,
        _ => false,
    };

    private async Task Close(CancellationToken cancellationToken)
    {
        if (_initialized == false)
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
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"), ex);
        }
    }

    #endregion

    #region Storage

    /// <inheritdoc />
    public async Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount)
    {
        EnsureInitialized();
        ArgumentOutOfRangeException.ThrowIfNegative(fromVersion);
        List<TLogEntry> entries = new();
        if (maxCount <= 0)
        {
            return entries;
        }

        double requestCharge = 0;
        var streamName = GetStreamName(grainId);
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.ReadOperation, grainTypeName, grainId, streamName);

        try
        {
            var sql = new QueryDefinition("SELECT * FROM c WHERE c.type = 'evt' AND c.ver > @fromVer AND c.ver <= @toVer ORDER BY c.ver ASC")
                .WithParameter("@fromVer", fromVersion)
                .WithParameter("@toVer", (long)fromVersion + maxCount);
            using (FeedIterator resultSetIterator = _container.GetItemQueryStreamIterator(
                sql,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(streamName),
                    MaxItemCount = _storageOptions.QueryMaxItemCount
                }))
            {
                while (resultSetIterator.HasMoreResults)
                {
                    using ResponseMessage response = await resultSetIterator.ReadNextAsync().ConfigureAwait(false);
                    requestCharge += response.Headers.RequestCharge;
                    operation.AddRequestCharge(response.Headers.RequestCharge);
                    response.EnsureSuccessStatusCode();
                    var page = await JsonSerializer.DeserializeAsync(response.Content, CosmosDBJsonContext.Default.EventItemQueryResponse).ConfigureAwait(false);
                    foreach (var item in page?.Documents ?? [])
                    {
                        if (!_typeMapper.TryGetType(item.DataType, out var evtType))
                        {
                            throw new InvalidOperationException($"Event type '{item.DataType}' in stream '{streamName}' with version '{item.Version}' is not registered.");
                        }
                        var data = JsonSerializer.SerializeToUtf8Bytes(item.Data, CosmosDBJsonContext.Default.JsonElement);
                        var entry = _storageSerializer.Deserialize<TLogEntry>(new BinaryDataWithType(data, evtType))
                            ?? throw new InvalidOperationException($"Event of type '{item.DataType}' in stream '{streamName}' with version '{item.Version}' deserialized to null.");
                        entries.Add(entry);
                    }
                }
            }

            operation.SetEventCount(entries.Count);
            LogRequestCharge(LogConsistentStorageInstrumentation.ReadOperation, streamName, entries.Count, requestCharge);

            return entries;
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            _logger.LogError(ex, "Failed to read log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
    {
        EnsureInitialized();

        var streamName = GetStreamName(grainId);
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.GetLastVersionOperation, grainTypeName, grainId, streamName);

        try
        {
            var result = await ReadHeaderItem(streamName).ConfigureAwait(false);
            operation.AddRequestCharge(result.RequestCharge);
            LogRequestCharge(LogConsistentStorageInstrumentation.GetLastVersionOperation, streamName, 0, result.RequestCharge);
            return result.EventItem is not null ? checked((int)result.EventItem.Version) : 0;
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            _logger.LogError(ex, "Failed to read last log entry for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read last log entry for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);

        var streamName = GetStreamName(grainId);

        if (entries.Count == 0)
        {
            return await GetLastVersionAsync(grainTypeName, grainId);
        }

        // The header and all events must be written in a single transactional batch so the
        // append is atomic. Cosmos DB limits a transactional batch to BatchSize operations.
        var maxEventsPerAppend = _storageOptions.BatchSize - 1;
        if (entries.Count > maxEventsPerAppend)
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Cannot append {entries.Count} entries atomically; the maximum is {maxEventsPerAppend}."));
        }

        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.AppendOperation, grainTypeName, grainId, streamName);
        try
        {
            return await AppendCoreAsync(grainTypeName, grainId, streamName, entries, expectedVersion, operation).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            throw;
        }
    }

    private async Task<int> AppendCoreAsync<TLogEntry>(string grainTypeName, GrainId grainId, string streamName, IList<TLogEntry> entries, int expectedVersion,
        LogConsistentStorageInstrumentation.Operation operation)
    {
        var transaction = _container.CreateTransactionalBatch(new PartitionKey(streamName));
        ulong currentVersion = (ulong)expectedVersion;

        if (currentVersion > 0)
        {
            var itemResponse = await ReadHeaderItem(streamName).ConfigureAwait(false);
            operation.AddRequestCharge(itemResponse.RequestCharge);

            var headerItem = itemResponse.EventItem
                ?? throw new InconsistentStateException(
                    $"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion} StoredVersion=0. Stream does not exist.",
                    "0", expectedVersion.ToString());
            if (headerItem.Deleted)
            {
                throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Stream is deleted."));
            }
            if (headerItem.Version != currentVersion)
            {
                throw new InconsistentStateException(
                    $"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion} StoredVersion={headerItem.Version}.",
                    headerItem.Version.ToString(), expectedVersion.ToString());
            }

            headerItem.Version += (ulong)entries.Count;

            _ = transaction.ReplaceItemStream(headerItem.Id, ToStream(headerItem), new TransactionalBatchItemRequestOptions
            {
                IfMatchEtag = headerItem.ETag,
            });
        }
        else
        {
            var header = new EventItem
            {
                Id = HeaderId,
                Type = EventItemType.Header,
                StreamName = streamName,
                Version = (ulong)entries.Count,
                DataType = HeaderDataType,
                Data = JsonSerializer.SerializeToElement(new StreamHeader(), CosmosDBJsonContext.Default.StreamHeader),
            };
            _ = transaction.CreateItemStream(ToStream(header));
        }

        ulong subSequenceNumber = 0;
        List<EventItem> eventItemsToAppend = new(entries.Count);

        foreach (var entry in entries)
        {
            if (entry is null)
            {
                throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Cannot append null entries."));
            }
            var eventDataEntry = new EventItem
            {
                Id = _eventIdGenerator().ToSequentialGuid().ToString(),
                Type = EventItemType.Event,
                StreamName = streamName,
                Version = ++currentVersion,
                SubSequenceNumber = subSequenceNumber++,
                DataType = _typeMapper.GetTypeName(entry.GetType()),
                Data = SerializeEntry(entry),
            };
            eventItemsToAppend.Add(eventDataEntry);
        }

        foreach (var item in eventItemsToAppend)
        {
            _ = transaction.CreateItemStream(ToStream(item));
        }

        using var response = await transaction.ExecuteAsync().ConfigureAwait(false);
        operation.AddRequestCharge(response.RequestCharge);
        var requestCharge = operation.RequestCharge;

        if (response.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.PreconditionFailed)
        {
            // The batch response does not report the stored version, so it is left unknown.
            throw new InconsistentStateException(
                $"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion}. StatusCode={response.StatusCode}.",
                null, expectedVersion.ToString());
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. RequestCharge: {requestCharge}, StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}"));
        }

        operation.SetEventCount(eventItemsToAppend.Count);
        LogRequestCharge(LogConsistentStorageInstrumentation.AppendOperation, streamName, eventItemsToAppend.Count, requestCharge);

        return checked((int)currentVersion);
    }

    private void LogRequestCharge(string operation, string streamName, int eventCount, double requestCharge)
    {
        if (requestCharge > _storageOptions.RequestChargeWarningThreshold)
        {
            _logger.LogWarning("Operation '{Operation}' on stream '{StreamName}' with '{EventCount}' events consumed '{RequestUnits}' RUs, exceeding the warning threshold of '{RequestChargeWarningThreshold}' RUs",
                operation, streamName, eventCount, requestCharge, _storageOptions.RequestChargeWarningThreshold);
        }
        else if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Operation '{Operation}' on stream '{StreamName}' with '{EventCount}' events consumed '{RequestUnits}' RUs",
                operation, streamName, eventCount, requestCharge);
        }
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new CosmosDBLogConsistentStorageException(FormattableString.Invariant($"CosmosDBLogConsistentStorage {_name} has not been initialized."));
        }
    }

    private async Task<EventItemResponse> ReadHeaderItem(string streamName)
    {
        using (var responseMessage = await _container.ReadItemStreamAsync(HeaderId, new PartitionKey(streamName)).ConfigureAwait(false))
        {
            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return new EventItemResponse(null, responseMessage.Headers.RequestCharge);
            }
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to read the header item for stream '{streamName}'. Reason: {responseMessage.ErrorMessage}");
            }
            var item = await JsonSerializer.DeserializeAsync(responseMessage.Content, CosmosDBJsonContext.Default.EventItem).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Found header item for stream '{streamName}' but failed to deserialize it.");
            return new EventItemResponse(item, responseMessage.Headers.RequestCharge);
        }
    }

    #endregion

    #region Serialize & Deserialize

    private JsonElement SerializeEntry<TLogEntry>(TLogEntry entry)
    {
        var data = _storageSerializer.Serialize(entry);
        try
        {
            using var document = JsonDocument.Parse(data.ToMemory());
            return document.RootElement.Clone();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"The configured {nameof(IGrainStorageSerializer)} must produce JSON to store events in Cosmos DB.", ex);
        }
    }

    private static MemoryStream ToStream(EventItem item)
        => new(JsonSerializer.SerializeToUtf8Bytes(item, CosmosDBJsonContext.Default.EventItem), writable: false);

    #endregion
}
