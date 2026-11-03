using Continuum.Events;
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

    /// <summary>
    ///     Initializes a storage created after silo startup, such as one for a tenant.
    /// </summary>
    internal Task InitializeAsync(CancellationToken cancellationToken) => Init(cancellationToken);

    private Task Init(CancellationToken cancellationToken)
    {
        // The client is resolved from the container in the constructor, and KurrentDB has no container or schema to
        // validate, so there is nothing to connect or create here. The client reconnects on its own and grain
        // activation retries reads, so a startup connectivity check would only make silo startup fragile.
        _initialized = true;
        _logger.StorageInitialized(_name, _serviceId);
        return Task.CompletedTask;
    }

    // The KurrentDBClient is a keyed singleton that other providers may share, so the DI container disposes it, not
    // this provider. There is nothing else to release.
    private static Task Close(CancellationToken cancellationToken) => Task.CompletedTask;

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
            _logger.ReadLogEntriesFailed(ex, grainTypeName, grainId, streamName);
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
            _logger.ReadLastLogEntryFailed(ex, grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read last log entry for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion, Guid transactionId)
    {
        EnsureInitialized();
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedVersion);
        var streamName = GetStreamName(grainId);
        if (entries.Count == 0)
        {
            return await GetLastVersionAsync(grainTypeName, grainId);
        }
        foreach (var entry in entries)
        {
            if (entry is null)
            {
                throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. Cannot append null entries."));
            }
        }
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.AppendOperation, grainTypeName, grainId, streamName);
        try
        {
            // expectedVersion is an event count; KurrentDB expects the zero based revision of the last event.
            StreamState streamState = expectedVersion == 0 ? StreamState.NoStream : StreamState.StreamRevision((ulong)(expectedVersion - 1));
            // Every entry of this append is committed together, but KurrentDB reports the same position for all of
            // them, so a subscriber cannot recover the grouping later. Recording it here is what lets a projection
            // apply the whole append at once instead of briefly serving a half applied state. A grain writes to its
            // own stream, so all of these entries belong to one stream and the two counts are the same. The writer
            // supplies the transaction ID so it can check whether an append that failed was actually committed.
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
            _logger.WriteLogEntriesFailed(ex, grainTypeName, grainId, streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
        }
    }

    /// <inheritdoc />
    public async Task<Guid?> GetTransactionIdAsync(string grainTypeName, GrainId grainId, int version)
    {
        EnsureInitialized();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        var streamName = GetStreamName(grainId);
        using var operation = _instrumentation.Start(LogConsistentStorageInstrumentation.GetTransactionIdOperation, grainTypeName, grainId, streamName);
        try
        {
            // The entry that produces a version is at the zero based revision one less.
            var readResult = _client.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.FromInt64(version - 1), 1, false, null, _userCredentials);
            if (await readResult.ReadState == ReadState.StreamNotFound)
            {
                return null;
            }
            await foreach (var resolvedEvent in readResult)
            {
                return KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span).TransactionId;
            }
            return null;
        }
        catch (Exception ex)
        {
            operation.Fail(ex);
            _logger.ReadTransactionIdFailed(ex, grainTypeName, grainId, streamName, version);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read the transaction ID of version {version} for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"), ex);
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
        var entryTypeName = _typeMapper.GetTypeName(entry!.GetType());
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

internal static partial class KurrentDBLogConsistentStorageLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Init: Name={Name} ServiceId={ServiceId}, initialized")]
    public static partial void StorageInitialized(this ILogger logger, string name, string serviceId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to read log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}")]
    public static partial void ReadLogEntriesFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string streamName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to read last log entry for {GrainType} grain with ID {GrainId} and stream {StreamName}")]
    public static partial void ReadLastLogEntryFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string streamName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to write log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}")]
    public static partial void WriteLogEntriesFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string streamName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to read the transaction ID of version {Version} for {GrainType} grain with ID {GrainId} and stream {StreamName}")]
    public static partial void ReadTransactionIdFailed(this ILogger logger, Exception exception, string grainType, GrainId grainId, string streamName, int version);
}
