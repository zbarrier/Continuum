using System.Diagnostics;

using Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;
using Continuum.Orleans.KurrentDB;
using Continuum.Serialization.Orleans;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Orleans.Configuration;
using Orleans.Storage;

namespace Continuum.EventSourcing.Orleans.KurrentDB;

/// <summary>
///     KurrentDB-based log consistent storage provider
/// </summary>
public class KurrentDBLogConsistentStorage : ILogConsistentStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly ILogger<KurrentDBLogConsistentStorage> _logger;
    private readonly string _name;
    private readonly string _serviceId;
    private readonly KurrentDBLogConsistentStorageOptions _storageOptions;
    private readonly IGrainStorageSerializer _storageSerializer;
    private readonly string _contentType;
    private readonly Func<string, string, GrainId, string> _streamNameFormatter;
    private readonly Func<Guid> _eventIdGenerator;
    private readonly ITypeMapper _typeMapper;
    private readonly UserCredentials? _userCredentials;
    private readonly KurrentDBClient _client;
    private readonly LogConsistentStorageInstrumentation _instrumentation;

    private bool _initialized = false;

    /// <summary>
    ///     Creates a new instance of the <see cref="KurrentDBLogConsistentStorage" /> type.
    /// </summary>
    public KurrentDBLogConsistentStorage(IServiceProvider serviceProvider,
        string name,
        KurrentDBLogConsistentStorageOptions storageOptions,
        IOptions<ClusterOptions> clusterOptions,
        ILogger<KurrentDBLogConsistentStorage> logger)
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
        _contentType = isJsonSerializer ? "application/json" : "application/octet-stream";

        _streamNameFormatter = _storageOptions.StreamNameFormatter;
        _eventIdGenerator = _storageOptions.EventIdGenerator;
        _typeMapper = _storageOptions.TypeMapper;
        _userCredentials = _storageOptions.Credentials.ToUserCredentials();

        _client = serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_storageOptions.ConnectionName);
        _instrumentation = new LogConsistentStorageInstrumentation(serviceProvider, "kurrentdb", name);
    }

    /// <summary>
    ///     Gets the KurrentDB stream name for a grain.
    /// </summary>
    /// <param name="grainId">The grain whose stream name is returned.</param>
    /// <returns>The stream name produced by <see cref="KurrentDBLogConsistentStorageOptions.StreamNameFormatter"/>.</returns>
    private string GetStreamName(GrainId grainId) => _streamNameFormatter(_serviceId, _name, grainId);

    #region Lifecycle Participant

    /// <inheritdoc />
    public void Participate(ISiloLifecycle lifecycle)
    {
        var name = OptionFormattingUtilities.Name<KurrentDBLogConsistentStorage>(_name);
        lifecycle.Subscribe(name, _storageOptions.InitStage, Init, Close);
    }

    private Task Init(CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();
        try
        {
            // The client is resolved from the container in the constructor, so there is nothing to connect or
            // create here. This is where startup work such as a connectivity check would go.
            _initialized = true;
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Init: Name={Name} ServiceId={ServiceId}, initialized in {ElapsedMilliseconds} ms", _name, _serviceId, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Init: Name={Name} ServiceId={ServiceId}, errored in {ElapsedMilliseconds} ms", _name, _serviceId, Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
            throw new KurrentDBStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"), ex);
        }
        return Task.CompletedTask;
    }

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
            //await _client.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Close: Name={Name} ServiceId={ServiceId}", _name, _serviceId);
            throw new KurrentDBStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"), ex);
        }
    }

    #endregion

    #region Storage

    /// <inheritdoc />
    public async Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount)
    {
        EnsureInitialized();
        ArgumentOutOfRangeException.ThrowIfNegative(fromVersion);
        if (maxCount <= 0)
        {
            return [];
        }
        var streamName = GetStreamName(grainId);
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.ReadOperation, grainTypeName, grainId, streamName);
        try
        {
            // fromVersion is an event count, which is also the revision of the next event to read.
            var streamPosition = StreamPosition.FromInt64(fromVersion);
            var readResult = _client.ReadStreamAsync(Direction.Forwards, streamName, streamPosition, maxCount, false, null, _userCredentials);
            var readState = await readResult.ReadState;
            if (readState == ReadState.StreamNotFound)
            {
                return [];
            }
            var entries = await readResult.Select(DeserializeEvent<TLogEntry>).ToListAsync();
            operation.SetEventCount(entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            _logger.LogError(ex, "Failed to read log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
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
            var readResult = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, false, null, _userCredentials);
            var readState = await readResult.ReadState;
            if (readState == ReadState.StreamNotFound)
            {
                return 0;
            }
            await foreach (var resolvedEvent in readResult)
            {
                // The last revision is zero based, so the event count is one more.
                return checked((int)(resolvedEvent.OriginalEventNumber.ToUInt64() + 1));
            }
            return 0;
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            _logger.LogError(ex, "Failed to read last log entry for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read last log entry for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
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
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.AppendOperation, grainTypeName, grainId, streamName);
        try
        {
            // expectedVersion is an event count; KurrentDB expects the zero based revision of the last event.
            StreamState streamState = expectedVersion == 0 ? StreamState.NoStream : StreamState.StreamRevision((ulong)(expectedVersion - 1));
            // Every entry of this append is committed together, but KurrentDB reports the same position for all of
            // them, so a subscriber cannot recover the grouping later. Recording it here is what lets a projection
            // apply the whole append at once instead of briefly serving a half applied state. A grain writes to its
            // own stream, so all of these entries belong to one stream and the two counts are the same.
            var transactionId = NewId.Next();
            var serializedEntries = new EventData[entries.Count];
            for (var index = 0; index < serializedEntries.Length; index++)
            {
                serializedEntries[index] = SerializeEvent(entries[index], new TransactionInfo(transactionId, entries.Count, entries.Count, index));
            }
            var writeResult = await _client.AppendToStreamAsync(streamName, streamState, serializedEntries, null, null, _userCredentials);
            operation.SetEventCount(serializedEntries.Length);
            // The next expected state is the revision of the last written event, so the event count is one more.
            return checked((int)(writeResult.NextExpectedStreamState.ToInt64() + 1));
        }
        catch (WrongExpectedVersionException ex)
        {
            operation.Fail(ex);
            // Versions are event counts, so the stored version is one more than the stream's last revision.
            var storedVersion = ex.ActualStreamState.HasPosition ? ex.ActualStreamState.ToInt64() + 1 : 0;
            throw new InconsistentStateException(
                $"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion} StoredVersion={storedVersion}.",
                storedVersion.ToString(), expectedVersion.ToString(), ex);
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            operation.Fail(ex);
            _logger.LogError(ex, "Failed to write log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    private void EnsureInitialized()
    {
        if (_initialized == false)
        {
            throw new InvalidOperationException(FormattableString.Invariant($"KurrentDB log consistent storage '{_name}' has not been initialized. Ensure the silo lifecycle has reached the configured init stage."));
        }
    }

    #endregion

    #region Serialize & Deserialize

    private readonly static ReadOnlyMemory<byte> ReadonlyEmptyData = new();

    /// <summary>
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="transaction">The append this entry belongs to, recorded so a subscriber can regroup the events.</param>
    /// <typeparam name="TLogEntry"></typeparam>
    /// <returns></returns>
    private EventData SerializeEvent<TLogEntry>(TLogEntry entry, TransactionInfo transaction)
    {
        var eventId = Uuid.FromGuid(_eventIdGenerator());
        var metadata = KurrentDBEventMetadataCodec.Write(null, transaction);
        var readonlyMetadata = metadata is null ? null : (ReadOnlyMemory<byte>?)metadata;
        if (entry is null)
        {
            var typeName = _typeMapper.GetTypeName<TLogEntry>();
            return new EventData(eventId, typeName, ReadonlyEmptyData, readonlyMetadata, _contentType);
        }
        var entryTypeName = _typeMapper.GetTypeName(entry.GetType());
        var entryData = _storageSerializer.Serialize(entry);
        var readonlyEntryData = entryData.ToMemory();
        return new EventData(eventId, entryTypeName, readonlyEntryData, readonlyMetadata, _contentType);
    }

    /// <summary>
    /// </summary>
    /// <param name="evt"></param>
    /// <typeparam name="TLogEntry"></typeparam>
    /// <returns></returns>
    private TLogEntry DeserializeEvent<TLogEntry>(ResolvedEvent evt)
    {
        var evtType = _typeMapper.GetType(evt.Event.EventType);
        var evtDataWithType = new BinaryDataWithType(evt.Event.Data, evtType);
        return _storageSerializer.Deserialize<TLogEntry>(evtDataWithType)
            ?? throw new InvalidOperationException($"Event of type '{evt.Event.EventType}' in stream '{evt.Event.EventStreamId}' with revision '{evt.Event.EventNumber}' deserialized to null.");
    }

    #endregion
}
