using System.Diagnostics;

using Continuum.Orleans.KurrentDB;
using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Orleans.Storage;

/// <summary>
///     KurrentDB-based grain storage provider
/// </summary>
public class KurrentDBGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly ILogger<KurrentDBGrainStorage> _logger;
    private readonly string _name;
    private readonly string _serviceId;
    private readonly KurrentDBGrainStorageOptions _storageOptions;
    private readonly IGrainStorageSerializer _storageSerializer;
    private readonly string _contentType;
    private readonly Func<string, string, GrainId, string> _streamNameFormatter;
    private readonly Func<Guid> _eventIdGenerator;
    private readonly ITypeMapper _typeMapper;
    private readonly UserCredentials? _userCredentials;
    private readonly KurrentDBClient _client;

    private bool _initialized;

    /// <summary>
    ///     Creates a new instance of the <see cref="KurrentDBGrainStorage" /> type.
    /// </summary>
    public KurrentDBGrainStorage(IServiceProvider serviceProvider, 
        string name,
        KurrentDBGrainStorageOptions storageOptions, 
        IOptions<ClusterOptions> clusterOptions, 
        ILogger<KurrentDBGrainStorage> logger)
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
        
        bool isJsonSerializer = _storageSerializer is TypeMappedJsonGrainStorageSerializer ||
                                _storageSerializer is SystemTextJsonGrainStorageSerializer ||
                                _storageSerializer is JsonGrainStorageSerializer;
        _contentType = _storageOptions.ContentType ?? (isJsonSerializer ? "application/json" : "application/octet-stream");

        _streamNameFormatter = _storageOptions.StreamNameFormatter;
        _eventIdGenerator = _storageOptions.EventIdGenerator;
        _typeMapper = _storageOptions.TypeMapper;
        _userCredentials = _storageOptions.Credentials.ToUserCredentials();
        _client = serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_storageOptions.ConnectionName);
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
        var name = OptionFormattingUtilities.Name<KurrentDBGrainStorage>(_name);
        lifecycle.Subscribe(name, _storageOptions.InitStage, Init, Close);
    }

    private Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            _logger.StorageInitializing(_name, _serviceId);
            _initialized = true;
            timer.Stop();
            _logger.StorageInitialized(_name, _serviceId, timer.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            timer.Stop();
            _logger.StorageInitializationFailed(ex, _name, _serviceId, timer.Elapsed.TotalMilliseconds);
            throw new KurrentDBStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"), ex);
        }
        return Task.CompletedTask;
    }

    private Task Close(CancellationToken cancellationToken)
    {
        // We don't need to dispose the client because it's a singleton, could be used by
        // other providers and will be disposed by the container.
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Initializes a storage that is created after the silo has started, such as a tenant's storage.
    /// </summary>
    internal Task InitializeAsync(CancellationToken cancellationToken) => Init(cancellationToken);

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"KurrentDBGrainStorage {_name} is not initialized. Storage cannot be used before the silo lifecycle stage {_storageOptions.InitStage} has started or after it has stopped.");
        }
    }

    #endregion

    #region Storage

    /// <inheritdoc />
    public async Task ReadStateAsync<T>(string grainTypeName, GrainId grainId, IGrainState<T> grainState)
    {
        EnsureInitialized();
        var streamName = GetStreamName(grainId);
        try
        {
            var latest = await ReadLatestEventAsync(streamName);
            if (latest is { } resolvedEvent)
            {
                // The stream still exists, so the ETag is kept for the next append even when the latest event is
                // the empty marker written by ClearStateAsync.
                grainState.ETag = resolvedEvent.Event.EventNumber.ToInt64().ToString();
                if (resolvedEvent.Event.EventType == ClearedEventType)
                {
                    grainState.RecordExists = false;
                }
                else
                {
                    grainState.State = DeserializeState<T>(resolvedEvent)!;
                    grainState.RecordExists = true;
                }
            }
            else
            {
                grainState.ETag = null;
                grainState.RecordExists = false;
            }
        }
        catch (Exception ex)
        {
            _logger.ReadStateFailed(ex, grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read grain state for {grainTypeName} with ID {grainId} and stream key {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task WriteStateAsync<T>(string grainTypeName, GrainId grainId, IGrainState<T> grainState)
    {
        EnsureInitialized();
        var streamName = GetStreamName(grainId);
        try
        {
            var eTagExists = ulong.TryParse(grainState.ETag, out var eTag);
            var expectedState = eTagExists ? StreamState.StreamRevision(eTag) : StreamState.NoStream;
            var serializedState = SerializeState(grainState.State);
            var eventData = new[] { serializedState };
            var writeResult = await _client.AppendToStreamAsync(streamName, expectedState, eventData, null, null, _userCredentials);
            if (writeResult.NextExpectedStreamState.HasPosition)
            {
                grainState.ETag = writeResult.NextExpectedStreamState.ToInt64().ToString();
            }
            grainState.RecordExists = true;
            if (!eTagExists)
            {
                await ApplyRetentionAsync(streamName);
            }
        }
        catch (WrongExpectedVersionException ex)
        {
            _logger.WriteStateVersionConflict(grainTypeName, grainId, streamName);
            throw CreateVersionConflict(nameof(WriteStateAsync), grainTypeName, grainId, grainState.ETag, ToETag(ex.ActualStreamState), ex);
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            _logger.WriteStateFailed(ex, grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to write grain state for {grainTypeName} with ID {grainId} and stream key {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task ClearStateAsync<T>(string grainTypeName, GrainId grainId, IGrainState<T> grainState)
    {
        EnsureInitialized();
        var streamName = GetStreamName(grainId);
        try
        {
            var eTagExists = ulong.TryParse(grainState.ETag, out var eTag);
            if (!eTagExists)
            {
                // This activation has not observed the state. If a stream exists anyway, another activation wrote it,
                // so the state is inconsistent and the grain should get a chance to deactivate and recover.
                var storedETag = await ReadCurrentETagAsync(streamName);
                if (storedETag is not null)
                {
                    _logger.ClearStateVersionConflict(grainTypeName, grainId, streamName);
                    throw CreateVersionConflict(nameof(ClearStateAsync), grainTypeName, grainId, grainState.ETag, storedETag, null);
                }
            }
            if (_storageOptions.DeleteStateOnClear)
            {
                if (eTagExists)
                {
                    var expectedState = StreamState.StreamRevision(eTag);
                    await _client.DeleteAsync(streamName, expectedState, null, _userCredentials);
                    // A soft-deleted stream reads as not found and accepts a NoStream append, so the next write
                    // must not expect the old revision.
                    grainState.ETag = null;
                }
            }
            else
            {
                if (eTagExists)
                {
                    var expectedState = StreamState.StreamRevision(eTag);
                    var serializedState = CreateClearedMarker();
                    var eventData = new[] { serializedState };
                    var writeResult = await _client.AppendToStreamAsync(streamName, expectedState, eventData, null, null, _userCredentials);
                    if (writeResult.NextExpectedStreamState.HasPosition)
                    {
                        grainState.ETag = writeResult.NextExpectedStreamState.ToInt64().ToString();
                    }
                }
            }
            grainState.RecordExists = false;
        }
        catch (WrongExpectedVersionException ex)
        {
            _logger.ClearStateVersionConflict(grainTypeName, grainId, streamName);
            throw CreateVersionConflict(nameof(ClearStateAsync), grainTypeName, grainId, grainState.ETag, ToETag(ex.ActualStreamState), ex);
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            _logger.ClearStateFailed(ex, grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to clear grain state for {grainTypeName} with ID {grainId} and stream key {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <summary>
    ///     Sets the stream's max count once the provider has created the stream. It runs after the append because
    ///     writing metadata to a soft-deleted stream brings the stream back, which would make the create append fail.
    ///     The other metadata is kept because a soft delete records where the recreated stream starts in it, and
    ///     dropping that would bring the deleted events back.
    /// </summary>
    private async Task ApplyRetentionAsync(string streamName)
    {
        if (_storageOptions.MaxStateEventCount is not { } maxCount)
        {
            return;
        }
        var current = await _client.GetStreamMetadataAsync(streamName, userCredentials: _userCredentials);
        var metadata = current.Metadata;
        if (metadata.MaxCount == maxCount)
        {
            return;
        }
        var updated = new StreamMetadata(maxCount, metadata.MaxAge, metadata.TruncateBefore, metadata.CacheControl, metadata.Acl, metadata.CustomMetadata);
        await _client.SetStreamMetadataAsync(streamName, StreamState.Any, updated, userCredentials: _userCredentials);
    }

    private async Task<string?> ReadCurrentETagAsync(string streamName)
    {
        var latest = await ReadLatestEventAsync(streamName);
        return latest?.Event.EventNumber.ToInt64().ToString();
    }

    private async Task<ResolvedEvent?> ReadLatestEventAsync(string streamName)
    {
        var backwards = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, false, null, _userCredentials);
        if (await backwards.ReadState == ReadState.Ok)
        {
            return await backwards.FirstOrDefaultAsync();
        }
        if (!_storageOptions.DeleteStateOnClear)
        {
            return null;
        }
        // Shortly after a soft-deleted stream is recreated, KurrentDB can report it as not found to a backwards
        // read while a forward read already returns the new events. Soft deletion hides the old events, so the
        // forward read only walks the events written since the stream was recreated.
        var forwards = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, long.MaxValue, false, null, _userCredentials);
        if (await forwards.ReadState != ReadState.Ok)
        {
            return null;
        }
        return await forwards.LastOrDefaultAsync();
    }

    private static string? ToETag(StreamState streamState) =>
        streamState.HasPosition ? streamState.ToInt64().ToString() : null;

    private InconsistentStateException CreateVersionConflict(string operation, string grainTypeName, GrainId grainId,
        string? currentETag, string? storedETag, Exception? innerException) =>
        new($"Version conflict ({operation}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} ETag={currentETag} StoredETag={storedETag}.",
            storedETag, currentETag, innerException);

    #endregion

    #region Serialize & Deserialize

    private readonly static ReadOnlyMemory<byte> ReadonlyEmptyData = new();

    /// <summary>
    ///     The event type of the empty marker appended by <see cref="ClearStateAsync{T}" />. A fixed name keeps
    ///     clearing independent of whether the state type is registered with the type mapper.
    /// </summary>
    private const string ClearedEventType = "continuum-state-cleared";

    private EventData CreateClearedMarker() =>
        new(Uuid.FromGuid(_eventIdGenerator()), ClearedEventType, ReadonlyEmptyData, null, _contentType);

    /// <summary>
    /// </summary>
    /// <param name="state"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private EventData SerializeState<T>(T state)
    {
        var eventId = Uuid.FromGuid(_eventIdGenerator());
        if (state is null)
        {
            return CreateClearedMarker();
        }
        var stateTypeName = _typeMapper.GetTypeName(state.GetType());
        var stateData = _storageSerializer.Serialize(state);
        var stateReadonlyData = stateData.ToMemory();
        return new EventData(eventId, stateTypeName, stateReadonlyData, null, _contentType);
    }

    /// <summary>
    /// </summary>
    /// <param name="evt"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T? DeserializeState<T>(ResolvedEvent evt)
    {
        var evtType = _typeMapper.GetType(evt.Event.EventType);
        var evtDataWithType = new BinaryDataWithType(evt.Event.Data, evtType);
        return _storageSerializer.Deserialize<T>(evtDataWithType);
    }

    #endregion

}

internal static partial class KurrentDBGrainStorageLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "KurrentDBGrainStorage {Name} is initializing: ServiceId={ServiceId}")]
    public static partial void StorageInitializing(this ILogger logger, string name, string serviceId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms")]
    public static partial void StorageInitialized(this ILogger logger, string name, string serviceId, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms")]
    public static partial void StorageInitializationFailed(this ILogger logger, Exception exception, string name, string serviceId, double elapsedMilliseconds);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to read grain state for {GrainType} grain with ID {GrainId} and stream key {Key}")]
    public static partial void ReadStateFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Version conflict for {GrainType} grain with ID {GrainId} and stream key {Key} on WriteStateAsync")]
    public static partial void WriteStateVersionConflict(this ILogger logger, string grainType, GrainId grainId, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to write grain state for {GrainType} grain with ID {GrainId} and stream key {Key}")]
    public static partial void WriteStateFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string key);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Version conflict for {GrainType} grain with ID {GrainId} and stream key {Key} on ClearStateAsync")]
    public static partial void ClearStateVersionConflict(this ILogger logger, string grainType, GrainId grainId, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to clear grain state for {GrainType} grain with ID {GrainId} and stream key {Key}")]
    public static partial void ClearStateFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string key);
}