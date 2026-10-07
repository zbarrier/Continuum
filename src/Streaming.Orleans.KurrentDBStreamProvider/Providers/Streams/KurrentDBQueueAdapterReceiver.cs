using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

using Continuum.Streaming.Orleans;

using KurrentDB.Client;

using Microsoft.Extensions.Logging;

using Orleans.Configuration;
using Orleans.Providers.Streams.Common;
using Orleans.Statistics;
using Orleans.Streaming.KurrentDBStorage;
using Orleans.Streams;

using StreamPosition = Orleans.Streams.StreamPosition;

namespace Orleans.Providers.Streams.KurrentDB;

/// <summary>
///     Receives batches of messages from a single queue of a message queue.
/// </summary>
internal class KurrentDBQueueAdapterReceiver : IQueueAdapterReceiver, IQueueCache
{
    public const int MaxMessagesPerRead = 1000;

    private readonly KurrentDBReceiverSettings _settings;
    private readonly Func<string, IStreamQueueCheckpointer<string>, ILoggerFactory, IKurrentDBQueueCache> _cacheFactory;
    private readonly Func<string, Task<IStreamQueueCheckpointer<string>>> _checkpointerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<KurrentDBQueueAdapterReceiver> _logger;
    private readonly IQueueAdapterReceiverMonitor _receiverMonitor;
    private readonly LoadSheddingOptions _loadSheddingOptions;
    private readonly IEnvironmentStatisticsProvider? _environmentStatisticsProvider;
    private readonly Func<KurrentDBReceiverSettings, string, ILogger, IKurrentDBReceiver> _kurrentDBReceiverFactory;

    private IKurrentDBQueueCache? _cache;
    private IKurrentDBReceiver? _receiver;
    private IStreamQueueCheckpointer<string>? _checkpointer;
    private AggregatedQueueFlowController? _flowController;

    // Receiver life cycle
    private int _receiverState = ReceiverShutdown;

    private const int ReceiverShutdown = 0;
    private const int ReceiverRunning = 1;

    public KurrentDBQueueAdapterReceiver(KurrentDBReceiverSettings settings, Func<string, IStreamQueueCheckpointer<string>, ILoggerFactory, IKurrentDBQueueCache> cacheFactory, Func<string, Task<IStreamQueueCheckpointer<string>>> checkpointerFactory, ILoggerFactory loggerFactory, IQueueAdapterReceiverMonitor receiverMonitor, LoadSheddingOptions loadSheddingOptions, IEnvironmentStatisticsProvider? environmentStatisticsProvider, Func<KurrentDBReceiverSettings, string, ILogger, IKurrentDBReceiver> kurrentDBReceiverFactory)
    {
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(cacheFactory, nameof(cacheFactory));
        ArgumentNullException.ThrowIfNull(checkpointerFactory, nameof(checkpointerFactory));
        ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
        ArgumentNullException.ThrowIfNull(receiverMonitor, nameof(receiverMonitor));
        ArgumentNullException.ThrowIfNull(loadSheddingOptions, nameof(loadSheddingOptions));
        // NOTE: required. A default that hard-codes the persistent subscription receiver would silently override the
        // subscription strategy selected through the stream configurator.
        ArgumentNullException.ThrowIfNull(kurrentDBReceiverFactory, nameof(kurrentDBReceiverFactory));
        _settings = settings;
        _cacheFactory = cacheFactory;
        _checkpointerFactory = checkpointerFactory;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<KurrentDBQueueAdapterReceiver>();
        _receiverMonitor = receiverMonitor;
        _loadSheddingOptions = loadSheddingOptions;
        _environmentStatisticsProvider = environmentStatisticsProvider;
        _kurrentDBReceiverFactory = kurrentDBReceiverFactory;
    }

    #region IQueueAdapterReceiver Implementation

    /// <inheritdoc />
    public Task Initialize(TimeSpan timeout)
    {
        _logger.LogInformation("Initializing KurrentDB persistent subscriptions from {ConsumerGroup}-{StreamName}.", _settings.ConsumerGroup, _settings.QueueName);
        // if receiver was already running, do nothing
        return ReceiverRunning == Interlocked.Exchange(ref _receiverState, ReceiverRunning) ? Task.CompletedTask : Initialize();
    }

    /// <summary>
    ///     Initialization of KurrentDB receiver is performed at adapter receiver initialization, but if it fails,
    ///     it will be retried when messages are requested
    /// </summary>
    /// <returns></returns>
    private async Task Initialize()
    {
        var watch = Stopwatch.StartNew();
        try
        {
            _checkpointer = await _checkpointerFactory(_settings.QueueName);
            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }
            _cache = _cacheFactory(_settings.QueueName, _checkpointer, _loggerFactory);
            // Purge is the point at which events are provably behind every cursor, so it is what drives the receiver's
            // checkpoint. Receive() is too early, since events there have only been buffered.
            _cache.OnCheckpointablePosition = commitPosition => NotifyDelivered(commitPosition);
            _flowController = new AggregatedQueueFlowController(MaxMessagesPerRead) { _cache };
            // Load shedding is only applied when environment statistics are available on the host.
            if (_environmentStatisticsProvider != null)
            {
                _flowController.Add(LoadShedQueueFlowController.CreateAsPercentOfLoadSheddingLimit(_loadSheddingOptions, _environmentStatisticsProvider));
            }
            var position = await _checkpointer.Load(CancellationToken.None);
            _receiver = _kurrentDBReceiverFactory(_settings, position, _logger);
            await _receiver.InitAsync();
            watch.Stop();
            _receiverMonitor.TrackInitialization(true, watch.Elapsed, null);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _receiverMonitor.TrackInitialization(false, watch.Elapsed, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task Shutdown(TimeSpan timeout)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            // if receiver was already shutdown, do nothing
            if (ReceiverShutdown == Interlocked.Exchange(ref _receiverState, ReceiverShutdown))
            {
                return;
            }
            _logger.LogInformation("Stopping reading from KurrentDB persistent subscriptions from {ConsumerGroup}-{StreamName}.", _settings.ConsumerGroup, _settings.QueueName);
            // clear cache and receiver
            var localCache = Interlocked.Exchange(ref _cache, null);
            var localReceiver = Interlocked.Exchange(ref _receiver, null);
            // start closing receiver
            var closeTask = Task.CompletedTask;
            if (localReceiver != null)
            {
                closeTask = localReceiver.CloseAsync();
            }
            // dispose of cache
            localCache?.Dispose();
            // finish return receiver closing task
            await closeTask;
            watch.Stop();
            _receiverMonitor.TrackShutdown(true, watch.Elapsed, null);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _receiverMonitor.TrackShutdown(false, watch.Elapsed, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IList<IBatchContainer>> GetQueueMessagesAsync(int maxCount)
    {
        if (_receiverState == ReceiverShutdown || maxCount <= 0)
        {
            return new List<IBatchContainer>();
        }
        // if receiver initialization failed, retry
        if (_receiver == null)
        {
            _logger.LogWarning(KurrentDBErrorCodes.CannotInitializeSubscriptionClient, "Retrying initialization of KurrentDB persistent subscriptions from {ConsumerGroup}-{StreamName}.", _settings.ConsumerGroup, _settings.QueueName);
            await Initialize();
            if (_receiver == null)
            {
                // should not get here, should throw instead, but just incase.
                return new List<IBatchContainer>();
            }
        }
        var watch = Stopwatch.StartNew();
        List<StreamedEvent<KurrentDBRawEvent>> messages;
        try
        {
            messages = _receiver.Receive(maxCount);
            watch.Stop();
            _receiverMonitor.TrackRead(true, watch.Elapsed, null);
        }
        catch (Exception ex)
        {
            watch.Stop();
            _receiverMonitor.TrackRead(false, watch.Elapsed, ex);
            _logger.LogWarning(KurrentDBErrorCodes.CannotReadFromSubscription, "Failed to read from KurrentDB persistent subscriptions from {ConsumerGroup}-{StreamName}. Exception: {Exception}", _settings.ConsumerGroup, _settings.QueueName, ex);
            throw;
        }
        var batches = new List<IBatchContainer>();
        if (messages == null || messages.Count == 0)
        {
            _receiverMonitor.TrackMessagesReceived(0, null, null);
            return batches;
        }
        // receiverMonitor message age
        var dequeueTimeUtc = DateTime.UtcNow;
        var oldestMessageEnqueueTime = messages[0].Timestamp;
        var newestMessageEnqueueTime = messages[^1].Timestamp;
        _receiverMonitor.TrackMessagesReceived(messages.Count, oldestMessageEnqueueTime, newestMessageEnqueueTime);
        if (_cache != null)
        {
            var messageStreamPositions = _cache.Add(messages, dequeueTimeUtc);
            batches.AddRange(messageStreamPositions.Select(streamPosition => new StreamActivityNotificationBatch(streamPosition)));
        }
        if (_checkpointer is { CheckpointExists: false })
        {
            // Seed the checkpoint with the commit position of the first event so a restart before the first purge
            // resumes from the start of this batch rather than from the beginning of the log.
            _checkpointer.Update(messages[0].SequenceNumber.ToString(), DateTime.UtcNow, CancellationToken.None);
        }
        return batches;
    }

    /// <inheritdoc />
    public Task MessagesDeliveredAsync(IList<IBatchContainer> messages)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Forwards a purged position to the receiver. Purging happens on the cache's synchronous path, so failures
    ///     are logged rather than surfaced; a missed checkpoint only causes a replay, never a loss.
    /// </summary>
    private void NotifyDelivered(ulong commitPosition)
    {
        var receiver = _receiver;
        if (receiver == null)
        {
            return;
        }
        _ = Task.Run(async () =>
        {
            try
            {
                await receiver.MessagesDeliveredAsync(commitPosition);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to checkpoint delivered position {CommitPosition} for queue {QueueName}.", commitPosition, _settings.QueueName);
            }
        });
    }

    #endregion

    #region IQueueCache Implementation

    /// <inheritdoc />
    public int GetMaxAddCount()
    {
        return _flowController?.GetMaxAddCount() ?? 0;
    }

    /// <inheritdoc />
    public void AddToCache(IList<IBatchContainer> messages)
    {
        // do nothing, we add data directly into cache.  No need for agent involvement
    }

    /// <inheritdoc />
    public bool TryPurgeFromCache([MaybeNullWhen(false)] out IList<IBatchContainer> purgedItems)
    {
        purgedItems = null;
        // if not under pressure, signal the cache to do a time based purge
        // if under pressure, which means consuming speed is less than producing speed, then shouldn't purge, and don't read more message into the cache
        if (!IsUnderPressure())
        {
            _cache?.SignalPurge();
        }
        return false;
    }

    /// <inheritdoc />
    public IQueueCacheCursor GetCacheCursor(StreamId streamId, StreamSequenceToken? token)
    {
        return new Cursor(_cache!, streamId, token);
    }

    /// <inheritdoc />
    public bool IsUnderPressure()
    {
        return GetMaxAddCount() <= 0;
    }

    #endregion

    #region Internal Class

    /// <summary>
    ///     Tells the pulling agent that a stream had activity, so it knows to read that stream from the queue cache.
    /// </summary>
    /// <remarks>
    ///     This deliberately carries no Orleans serialization attributes. It never leaves the pulling agent: the agent
    ///     responds to it by reading the real events from the cache through an <see cref="IQueueCacheCursor" />, and
    ///     those batches are what get delivered to consumers. Marking it serializable would be misleading, because
    ///     <see cref="StreamPosition" /> is a plain framework class with no codec and would fail to serialize anyway.
    /// </remarks>
    internal class StreamActivityNotificationBatch : IBatchContainer
    {
        public StreamActivityNotificationBatch(StreamPosition position)
        {
            Position = position;
        }

        public StreamPosition Position { get; }

        public StreamId StreamId => Position.StreamId;

        public StreamSequenceToken SequenceToken => Position.SequenceToken;

        public IEnumerable<Tuple<T, StreamSequenceToken>> GetEvents<T>()
        {
            // This batch is a signal that a stream had activity, not a carrier of events. The agent responds by
            // reading the real events from the cache through a cursor, so nothing should ever ask this for events.
            throw new NotSupportedException($"{nameof(StreamActivityNotificationBatch)} reports stream activity and carries no events. Events for this stream are read from the queue cache through {nameof(IQueueCacheCursor)}.");
        }

        public bool ImportRequestContext()
        {
            throw new NotSupportedException($"{nameof(StreamActivityNotificationBatch)} reports stream activity and carries no request context.");
        }
    }

    private class Cursor : IQueueCacheCursor
    {
        private readonly IKurrentDBQueueCache _cache;
        private readonly object _cursor;
        private IBatchContainer _current;
        private bool _deliveryFailed;

        public Cursor(IKurrentDBQueueCache cache, StreamId streamId, StreamSequenceToken? token)
        {
            _cache = cache;
            _cursor = cache.GetCursor(streamId, token);
            _current = null!;
        }

        public void Dispose()
        {
            // Releasing the cursor lets the cache purge, and therefore checkpoint, past what this cursor had read.
            _cache.ReleaseCursor(_cursor);
        }

        public IBatchContainer GetCurrent(out Exception? exception)
        {
            exception = null;
            return _current;
        }

        public bool MoveNext()
        {
            // Asking for the next event is the only signal the agent gives that it is finished with the current one,
            // so the watermark advances here rather than when the event was handed out. An event whose delivery failed
            // keeps the watermark where it is, so it is not purged or checkpointed away and survives a restart.
            CommitCurrent();
            if (!_cache.TryGetNextMessage(_cursor, out var next))
            {
                return false;
            }
            _current = next;
            return true;
        }

        private void CommitCurrent()
        {
            if (!_deliveryFailed && _current is IKurrentDBCommitPositionBatch delivered)
            {
                _cache.RecordDelivered(_cursor, delivered.CommitPosition);
            }
            _deliveryFailed = false;
        }

        public void Refresh(StreamSequenceToken token)
        {
        }

        public void RecordDeliveryFailure()
        {
            // Holds the watermark at the last successfully delivered event so this one is retained for redelivery.
            _deliveryFailed = true;
        }
    }

    #endregion

}
