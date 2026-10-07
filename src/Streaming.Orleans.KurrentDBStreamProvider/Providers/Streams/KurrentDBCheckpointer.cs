using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Serialization;
using Orleans.Streaming.KurrentDBStorage;
using Orleans.Streams;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     This class stores KurrentDB queue checkpointer information (a stream position) in another KurrentDB stream.
/// </summary>
public class KurrentDBCheckpointer : IStreamQueueCheckpointer<string>
{
    // private readonly ILogger<KurrentDBCheckpointer> _logger;
    private readonly TimeSpan _persistInterval;
    private readonly KurrentDBStateManager _stateManager;
    private KurrentDBCheckpointState _checkPointState;
    private readonly string _streamName;

    private Task? _inProgressSaveTask;
    private DateTime? _throttleSavesUntilUtc;

    /// <summary>
    ///     A factory method that creates a new instance of <see cref="KurrentDBCheckpointer" /> with the specified parameters.
    /// </summary>
    /// <param name="serviceId">The ID of the service that the checkpointer will be created for.</param>
    /// <param name="streamProviderName">The name of the stream provider that the checkpointer will be created for.</param>
    /// <param name="queue">The name of the queue that the checkpointer will be created for.</param>
    /// <param name="options">The options used to configure the checkpointer.</param>
    /// <param name="serviceProvider">Used to resolve the shared keyed client when a connection name is configured.</param>
    /// <param name="serializer">The serializer used for state manager.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    /// <returns>A new instance of <see cref="KurrentDBCheckpointer" />.</returns>
    public static KurrentDBCheckpointer Create(string serviceId, string streamProviderName, string queue, KurrentDBStreamCheckpointerOptions options, IServiceProvider? serviceProvider, Serializer serializer, ILoggerFactory loggerFactory)
    {
        var checkpointer = new KurrentDBCheckpointer(serviceId, streamProviderName, queue, options, serviceProvider, serializer, loggerFactory);
        checkpointer.Initialize();
        return checkpointer;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="KurrentDBCheckpointer" /> class.
    /// </summary>
    /// <param name="serviceId">The ID of the service that the checkpointer will be created for.</param>
    /// <param name="streamProviderName">The name of the stream provider that the checkpointer will be created for.</param>
    /// <param name="queue">The name of the queue that the checkpointer will be created for.</param>
    /// <param name="options">The options used to configure the checkpointer.</param>
    /// <param name="serviceProvider">Used to resolve the shared keyed client when a connection name is configured.</param>
    /// <param name="serializer">The serializer used for state manager.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    private KurrentDBCheckpointer(string serviceId, string streamProviderName, string queue, KurrentDBStreamCheckpointerOptions options, IServiceProvider? serviceProvider, Serializer serializer, ILoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceId, nameof(serviceId));
        ArgumentException.ThrowIfNullOrEmpty(streamProviderName, nameof(streamProviderName));
        ArgumentException.ThrowIfNullOrEmpty(queue, nameof(queue));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        // _logger = loggerFactory.CreateLogger<KurrentDBCheckpointer>();
        // _logger.LogInformation("Creating KurrentDB checkpointer for queue {Queue} of stream provider {StreamProviderName} with serviceId {ServiceId}.", queue, streamProviderName, serviceId);
        _persistInterval = options.PersistInterval;
        _stateManager = new KurrentDBStateManager(options, serviceProvider, serializer, loggerFactory.CreateLogger<KurrentDBStateManager>());
        _checkPointState = new KurrentDBCheckpointState();
        _streamName = KurrentDBCheckpointState.GetStreamName(serviceId, streamProviderName, queue);
    }

    private void Initialize()
    {
        _stateManager.Init();
    }

    /// <summary>
    ///     Gets a value indicating whether a checkpoint exists.
    /// </summary>
    /// <value><see langword="true" /> if checkpoint exists; otherwise, <see langword="false" />.</value>
    public bool CheckpointExists => _checkPointState is { Position: { } } && !string.IsNullOrEmpty(_checkPointState.Position);

    /// <summary>
    ///     Loads the checkpoint.
    /// </summary>
    /// <returns>The checkpoint.</returns>
    public Task<string> Load() => Load(CancellationToken.None);

    /// <summary>
    ///     Loads the checkpoint.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The checkpoint.</returns>
    public async Task<string> Load(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var checkpointState = await _stateManager.ReadStateAsync<KurrentDBCheckpointState>(_streamName);
        if (checkpointState != null)
        {
            _checkPointState = checkpointState;
        }
        return _checkPointState.Position;
    }

    /// <summary>
    ///     Updates the checkpoint.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="utcNow">The current UTC time.</param>
    public void Update(string position, DateTime utcNow) => Update(position, utcNow, CancellationToken.None);

    /// <summary>
    ///     Updates the checkpoint.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="utcNow">The current UTC time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public void Update(string position, DateTime utcNow, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // if position has not changed, do nothing
        if (string.Compare(_checkPointState.Position, position, StringComparison.Ordinal) == 0)
        {
            return;
        }
        // if we've saved before but it's not time for another save or the last save operation has not completed, do nothing
        if (_inProgressSaveTask != null && _throttleSavesUntilUtc.HasValue && (_throttleSavesUntilUtc.Value > utcNow || !_inProgressSaveTask.IsCompleted))
        {
            return;
        }
        _checkPointState.Position = position;
        _throttleSavesUntilUtc = utcNow + _persistInterval;
        _inProgressSaveTask = _stateManager.WriteStateAsync(_streamName, _checkPointState, true);
        _inProgressSaveTask.Ignore();
    }
}
