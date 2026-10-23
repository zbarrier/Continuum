using System.Globalization;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Storage;

namespace Continuum.Streaming.Orleans.KurrentDB.StreamProvider.Storage;

/// <summary>
///     Utility class to encapsulate data access to KurrentDB storage.
/// </summary>
public class KurrentDBStateManager : IDisposable, IAsyncDisposable
{
    private readonly KurrentDBOperationOptions _options;
    private readonly KurrentDBPolicyOptions _policyOptions;
    private readonly UserCredentials? _credentials;
    private readonly IServiceProvider _serviceProvider;
    private readonly Serializer _serializer;
    private readonly ILogger _logger;

    // The client resolved by ConnectionName is a container owned keyed singleton shared with the rest of the provider,
    // so it must not be disposed here.
    private KurrentDBClient? _client;
    private bool _initialized;

    /// <summary>
    ///     Creates a new <see cref="KurrentDBStateManager" /> instance.
    /// </summary>
    /// <param name="options">Storage configuration.</param>
    /// <param name="serviceProvider">Used to resolve the shared keyed client registered for the connection name.</param>
    /// <param name="serializer"></param>
    /// <param name="logger">Logger to use.</param>
    public KurrentDBStateManager(KurrentDBOperationOptions options, IServiceProvider serviceProvider, Serializer serializer, ILogger<KurrentDBStateManager> logger)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConnectionName, nameof(options.ConnectionName));
        ArgumentNullException.ThrowIfNull(options.PolicyOptions, nameof(options.PolicyOptions));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _options = options;
        _serviceProvider = serviceProvider;
        _policyOptions = options.PolicyOptions;
        _credentials = options.Credentials?.ToUserCredentials();
        _serializer = serializer;
        _logger = logger;
    }

    #region Lifecycle

    /// <summary>
    ///     Resolves the shared KurrentDB client.
    /// </summary>
    public void Init()
    {
        const string operation = "Init";
        var startTime = DateTime.UtcNow;
        try
        {
            _client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_options.ConnectionName);
            _initialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(KurrentDBErrorCodes.FailCreatingClient, ex, "Failed to init KurrentDB client.");
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to init KurrentDB client, {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            CheckAlertSlowAccess(startTime, operation);
        }
    }

    /// <summary>
    ///     Releases the shared KurrentDB client. The client is owned by the container and is not disposed.
    /// </summary>
    public Task Close()
    {
        Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client = null;
        _initialized = false;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Close().ConfigureAwait(false);
    }

    #endregion

    #region Storage

    /// <summary>
    ///     Reads a state entry from the KurrentDB stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    public async Task<T?> ReadStateAsync<T>(string streamName)
        where T : class, IKurrentDBState, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(streamName, nameof(streamName));
        if (_initialized == false || _client == null)
        {
            _logger.LogWarning(KurrentDBErrorCodes.CannotInitializeClient, "KurrentDB client for stream {StreamName} is not initialized.", streamName);
            throw new InvalidOperationException(FormattableString.Invariant($"KurrentDB client for stream {streamName} is not initialized."));
        }
        const string operation = "ReadState";
        var startTime = DateTime.UtcNow;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("{Operation} entry {State} of stream {StreamName}", operation, typeof(T), streamName);
        }
        try
        {
            var readResult = _client.ReadStreamAsync(Direction.Backwards, streamName, StreamPosition.End, 1, false, null, _credentials);
            var readState = await readResult.ReadState.ConfigureAwait(false);
            if (readState == ReadState.Ok)
            {
                var resolvedEvent = await readResult.FirstOrDefaultAsync().ConfigureAwait(false);
                var deserializeState = DeserializeState<T>(resolvedEvent);
                var state = deserializeState.State;
                state.ETag = deserializeState.ETag;
                return state;
            }
            else
            {
                _logger.LogWarning(KurrentDBErrorCodes.CannotReadStateFromStream, "Failed to read state for stream {StreamName}.", streamName);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(KurrentDBErrorCodes.CannotReadStateFromStream, "Failed to read state for stream {StreamName}.", streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to read state for stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            CheckAlertSlowAccess(startTime, operation, streamName);
        }
    }

    /// <summary>
    ///     Append a state entry to the KurrentDB stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="state">State to be appended to the stream.</param>
    /// <param name="ignoreETag">If ETag should be checked.</param>
    public async Task<T> WriteStateAsync<T>(string streamName, T state, bool ignoreETag = false)
        where T : class, IKurrentDBState, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(streamName, nameof(streamName));
        if (_initialized == false || _client == null)
        {
            _logger.LogWarning(KurrentDBErrorCodes.CannotInitializeClient, "KurrentDB client for stream {StreamName} is not initialized.", streamName);
            throw new InvalidOperationException(FormattableString.Invariant($"KurrentDB client for stream {streamName} is not initialized."));
        }
        const string operation = "WriteState";
        var startTime = DateTime.UtcNow;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("{Operation} entry {State} of stream {StreamName}", operation, state, streamName);
        }
        try
        {
            var eTagExists = ulong.TryParse(state.ETag, out var eTag);
            var serializedState = SerializeState(state);
            IWriteResult writeResult;
            if (ignoreETag)
            {
                writeResult = await _client.AppendToStreamAsync(streamName, StreamState.Any, new[] { serializedState }, null, null, _credentials).ConfigureAwait(false);
            }
            else
            {
                writeResult = await _client.AppendToStreamAsync(streamName, eTagExists ? StreamState.StreamRevision(eTag) : StreamState.NoStream, new[] { serializedState }, null, null, _credentials).ConfigureAwait(false);
            }
            state.ETag = writeResult.NextExpectedStreamState.ToInt64().ToString(CultureInfo.InvariantCulture);
            return state;
        }
        catch (WrongExpectedVersionException)
        {
            _logger.LogWarning(KurrentDBErrorCodes.VersionConflictInStream, "Version conflict for stream {StreamName} when write state.", streamName);
            throw new InconsistentStateException($"Version conflict when write state: ETag={state.ETag}.");
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            _logger.LogError(KurrentDBErrorCodes.CannotWriteStateToStream, "Failed to write state for stream {StreamName}.", streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to write state for stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            CheckAlertSlowAccess(startTime, operation, streamName);
        }
    }

    /// <summary>
    ///     Clear a state entry of the KurrentDB stream.
    /// </summary>
    /// <param name="streamName">The stream name.</param>
    /// <param name="state">State to be cleared of the stream.</param>
    /// <param name="ignoreETag">If ETag should be checked.</param>
    public async Task ClearStateAsync<T>(string streamName, T state, bool ignoreETag)
        where T : class, IKurrentDBState, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(streamName, nameof(streamName));
        if (_initialized == false || _client == null)
        {
            _logger.LogWarning(KurrentDBErrorCodes.CannotInitializeClient, "KurrentDB client for stream {StreamName} is not initialized.", streamName);
            throw new InvalidOperationException(FormattableString.Invariant($"KurrentDB client for stream {streamName} is not initialized."));
        }
        const string operation = "ClearState";
        var startTime = DateTime.UtcNow;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("{Operation} entry {State} of stream {StreamName}", operation, state, streamName);
        }
        try
        {
            var eTagExists = ulong.TryParse(state.ETag, out var eTag);
            if (!ignoreETag && eTagExists)
            {
                await _client.DeleteAsync(streamName, StreamState.StreamRevision(eTag), null, _credentials).ConfigureAwait(false);
                return;
            }
            await _client.DeleteAsync(streamName, StreamState.Any, null, _credentials).ConfigureAwait(false);
        }
        catch (WrongExpectedVersionException)
        {
            _logger.LogWarning(KurrentDBErrorCodes.VersionConflictInStream, "Version conflict for stream {StreamName} when clear state.", streamName);
            throw new InconsistentStateException($"Version conflict when clear state: ETag={state.ETag}.");
        }
        catch (Exception ex) when (ex is not InconsistentStateException)
        {
            _logger.LogError(KurrentDBErrorCodes.CannotClearStateToStream, "Failed to clear state for stream {StreamName}.", streamName);
            throw new KurrentDBStorageException(FormattableString.Invariant($"Failed to clear state for stream {streamName}. {ex.GetType()}: {ex.Message}"));
        }
        finally
        {
            CheckAlertSlowAccess(startTime, operation, streamName);
        }
    }

    #endregion

    #region Helper

    private void CheckAlertSlowAccess(DateTime startOperation, string operation, string? streamName = null)
    {
        var timeSpan = DateTime.UtcNow - startOperation;
        if (timeSpan <= _policyOptions.OperationTimeout)
        {
            return;
        }
        if (streamName == null)
        {
            _logger.LogWarning(KurrentDBErrorCodes.SlowAccessToStream, "Slow access to KurrentDB for {Operation}, which took {Duration}", operation, timeSpan);
        }
        else
        {
            _logger.LogWarning(KurrentDBErrorCodes.SlowAccessToStream, "Slow access to KurrentDB stream {StreamName} for {Operation}, which took {Duration}", streamName, operation, timeSpan);
        }
    }

    #endregion

    #region Serialize & Deserialize

    /// <summary>
    ///     Serializes the provided KurrentDB state object into an event data instance.
    /// </summary>
    /// <typeparam name="T">The type of the KurrentDB state object being serialized.</typeparam>
    /// <param name="state">The KurrentDB state object being serialized.</param>
    /// <returns>An event data instance representing the serialized state object.</returns>
    private EventData SerializeState<T>(T state)
        where T : class, IKurrentDBState, new()
    {
        if (state is null)
        {
            return new EventData(Uuid.NewUuid(), typeof(T).Name, new ReadOnlyMemory<byte>(), null, "application/octet-stream");
        }
        var stateBuffer = _serializer.SerializeToArray(state);
        return new EventData(Uuid.NewUuid(), state.GetType().Name, new ReadOnlyMemory<byte>(stateBuffer), null, "application/octet-stream");
    }

    /// <summary>
    ///     Deserializes the provided event data into an KurrentDB state object.
    /// </summary>
    /// <typeparam name="T">The type of the KurrentDB state object being deserialized.</typeparam>
    /// <param name="evt">The resolved event containing the serialized state data.</param>
    /// <returns>A tuple containing the deserialized state object and its ETag.</returns>
    private (T State, string ETag) DeserializeState<T>(ResolvedEvent evt)
        where T : class, IKurrentDBState, new()
    {
        var state = _serializer.Deserialize<T>(evt.Event.Data) ?? Activator.CreateInstance<T>();
        return (state, evt.Event.EventNumber.ToUInt64().ToString());
    }

    #endregion

}
