using System.Diagnostics;

using Continuum.EventSourcing.Orleans.KurrentDB.Abstractions;
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

        bool isJsonSerializer = _storageSerializer is SystemTextJsonGrainStorageSerializer ||
                                _storageSerializer is JsonGrainStorageSerializer;
        _contentType = isJsonSerializer ? "application/json" : "application/octet-stream";

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
        var name = OptionFormattingUtilities.Name<KurrentDBLogConsistentStorage>(_name);
        lifecycle.Subscribe(name, _storageOptions.InitStage, Init, Close);
    }

    private Task Init(CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("KurrentDBLogConsistentStorage {Name} is initializing: ServiceId={ServiceId}", _name, _serviceId);
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
            throw new KurrentDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"));
        }
        return Task.CompletedTask;
    }

    private async Task Close(CancellationToken cancellationToken)
    {
        if (_initialized == false || _client is null)
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
            throw new KurrentDBLogConsistentStorageException(FormattableString.Invariant($"{ex.GetType()}: {ex.Message}"));
        }
    }

    #endregion

    #region Storage

    /// <inheritdoc />
    public async Task<IReadOnlyList<TLogEntry>> ReadAsync<TLogEntry>(string grainTypeName, GrainId grainId, int fromVersion, int maxCount)
    {
        if (_initialized == false || _client is null || maxCount <= 0)
        {
            return new List<TLogEntry>();
        }
        var streamName = GetStreamName(grainId);
        try
        {
            var streamPosition = StreamPosition.FromInt64(fromVersion);
            var readResult = _client.ReadStreamAsync(Direction.Forwards, streamName, streamPosition, maxCount, false, null, _userCredentials);
            var readState = await readResult.ReadState;
            if (readState == ReadState.StreamNotFound)
            {
                return new List<TLogEntry>();
            }
            return await readResult.Select(DeserializeEvent<TLogEntry>).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<int> GetLastVersionAsync(string grainTypeName, GrainId grainId)
    {
        if (_initialized == false || _client is null)
        {
            return -1;
        }
        var streamName = GetStreamName(grainId);
        try
        {
            var readResult = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, false, null, _userCredentials);
            var readState = await readResult.ReadState;
            if (readState == ReadState.StreamNotFound)
            {
                return -1;
            }
            var resolvedEvent = await readResult.FirstOrDefaultAsync();
            return (int)resolvedEvent.Event.EventNumber.ToUInt64();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read last log entry for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBLogConsistentStorageException(FormattableString.Invariant($"Failed to read last log entry for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<int> AppendAsync<TLogEntry>(string grainTypeName, GrainId grainId, IList<TLogEntry> entries, int expectedVersion)
    {
        if (_initialized == false || _client is null)
        {
            return -1;
        }
        var streamName = GetStreamName(grainId);
        if (entries.Count == 0)
        {
            return await GetLastVersionAsync(grainTypeName, grainId);
        }
        try
        {
            StreamState streamState = expectedVersion == -1 ? StreamState.NoStream : StreamState.StreamRevision((ulong)expectedVersion);
            // Every entry of this append is committed together, but KurrentDB reports the same position for all of
            // them, so a subscriber cannot recover the grouping later. Recording it here is what lets a projection
            // apply the whole append at once instead of briefly serving a half applied state. A grain writes to its
            // own stream, so all of these entries belong to one stream and the two counts are the same.
            var transactionId = NewId.Next();
            var serializedEntries = entries.Select((entry, index) =>
                SerializeEvent(entry, new KurrentDBEventMetadataCodec.TransactionInfo(transactionId, entries.Count, entries.Count, index))).ToList();
            var writeResult = await _client.AppendToStreamAsync(streamName, streamState, serializedEntries, null, null, _userCredentials);
            long nextExpectedVersion = writeResult.NextExpectedStreamState.ToInt64();
            return (int)nextExpectedVersion;
        }
        catch (WrongExpectedVersionException)
        {
            throw new InconsistentStateException($"Version conflict ({nameof(AppendAsync)}): ServiceId={_serviceId} ProviderName={_name} GrainType={grainTypeName} GrainId={grainId} Version={expectedVersion}.");
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            _logger.LogError("Failed to write log entries for {GrainType} grain with ID {GrainId} and stream {StreamName}", grainTypeName, grainId, streamName);
            throw new KurrentDBLogConsistentStorageException(FormattableString.Invariant($"Failed to write log entries for {grainTypeName} with ID {grainId} and stream {streamName}. {ex.GetType()}: {ex.Message}"));
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
    private EventData SerializeEvent<TLogEntry>(TLogEntry entry, KurrentDBEventMetadataCodec.TransactionInfo transaction)
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
        return _storageSerializer.Deserialize<TLogEntry>(evtDataWithType);
    }

    #endregion
}
