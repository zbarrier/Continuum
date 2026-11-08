using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

using Continuum.Orleans.KurrentDB;
using Continuum.Streaming.Orleans.KurrentDB.Extensions;
using Continuum.Streaming.Orleans.KurrentDB.Monitors;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Configuration;
using Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions.Diagnostics;
using Continuum.TypeMapping;

using KurrentDB.Client;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Grpc.Core;

using Orleans.Runtime;
using Orleans.Storage;

using Polly;
using Polly.Retry;

namespace Continuum.Streaming.Orleans.KurrentDB.SelfHostedSubscriptions;

/// <summary>
///     Identifies a catch-up subscription for registration.
/// </summary>
/// <param name="Name">The subscription name, which is also its options name.</param>
/// <param name="Type">The subscription type.</param>
public abstract record KurrentDBCatchupSubscriptionInfo(string Name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)][property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type Type);

/// <summary>
///     Identifies a catch-up subscription to <c>$all</c> for registration.
/// </summary>
/// <param name="Name">The subscription name, which is also its options name.</param>
/// <param name="Type">The subscription type.</param>
public sealed record KurrentDBAllStreamCatchupSubscriptionInfo(string Name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type Type)
    : KurrentDBCatchupSubscriptionInfo(Name, Type);

/// <summary>
///     Identifies a catch-up subscription to a single stream for registration.
/// </summary>
/// <param name="Name">The subscription name, which is also its options name.</param>
/// <param name="Type">The subscription type.</param>
public sealed record KurrentDBStreamCatchupSubscriptionInfo(string Name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type Type)
    : KurrentDBCatchupSubscriptionInfo(Name, Type);

/// <summary>
///     The shared implementation of a hosted catch-up subscription that reads KurrentDB and delivers events in batches.
///     Derive from <see cref="KurrentDBAllStreamCatchupSubscription{TSubscription, TOptions}" /> or
///     <see cref="KurrentDBStreamCatchupSubscription{TSubscription, TOptions}" />.
/// </summary>
/// <typeparam name="TSubscription">The concrete subscription type.</typeparam>
/// <typeparam name="TOptions">The subscription options type.</typeparam>
public abstract class KurrentDBCatchupSubscription<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSubscription, TOptions> : BackgroundService, IStreamSubscription
    where TSubscription : KurrentDBCatchupSubscription<TSubscription, TOptions>
    where TOptions : KurrentDBCatchupSubscriptionOptions, new()
{
    /// <summary>
    ///     The subscription name, derived from <typeparamref name="TSubscription" /> without a <c>CatchupSubscription</c> suffix.
    /// </summary>
    public static readonly string Name;

    /// <summary>
    ///     The subscription type.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public static readonly Type Type = typeof(TSubscription);
    static KurrentDBCatchupSubscription()
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

    private readonly IServiceProvider _serviceProvider;

    /// <summary>The logger.</summary>
    protected readonly ILogger<TSubscription> _logger;
    /// <summary>The subscription options.</summary>
    protected readonly TOptions _options;
    /// <summary>The KurrentDB client the subscription reads from.</summary>
    protected readonly KurrentDBClient _client;
    /// <summary>The monitor that commits the subscription's checkpoint.</summary>
    protected readonly KurrentDBCheckpointMonitor _checkpointMonitor;
    /// <summary>The serializer used to deserialize event payloads.</summary>
    protected readonly IGrainStorageSerializer _storageSerializer;
    /// <summary>The type mapper used to resolve event types.</summary>
    protected readonly ITypeMapper _typeMapper;

    /// <summary>The channel buffering events between the subscription and the batch processor.</summary>
    protected readonly Channel<StreamedEvent<object>> _channel;

    /// <summary>The task processing events from <see cref="_channel" />.</summary>
    protected Task? _receivingTask = null;

    private readonly KurrentDBCatchupSubscriptionStatusRegistry _statusRegistry;
    private readonly KurrentDBCatchupSubscriptionMetrics _metrics;
    private volatile KurrentDBCatchupSubscriptionState _state = KurrentDBCatchupSubscriptionState.Starting;


    /// <summary>
    ///     Initializes a new instance of the subscription.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve options and dependencies.</param>
    private protected KurrentDBCatchupSubscription(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<TSubscription>>();
        _options = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>().Get(Name);
        _client = _serviceProvider.GetRequiredKeyedService<KurrentDBClient>(_options.ConnectionName);
        _checkpointMonitor = new KurrentDBCheckpointMonitor(_logger, _options.CheckpointMonitorOptions, _options.CheckpointStore,
            SubscriptionName, StreamName);
        _storageSerializer = _options.GrainStorageSerializer;
        _typeMapper = _options.TypeMapper;
        _statusRegistry = serviceProvider.GetRequiredService<KurrentDBCatchupSubscriptionStatusRegistry>();
        _metrics = serviceProvider.GetRequiredService<KurrentDBCatchupSubscriptionMetrics>();

        _channel = Channel.CreateBounded<StreamedEvent<object>>(new BoundedChannelOptions(_options.BufferOptions.MaxUnprocessedEventsBeforeStop)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,

            // This is the default value, but being explicit for clarity.  This allows the writer to continue writing
            // to the channel without being blocked by a slow reader, while still ensuring that the reader processes
            // events in a timely manner.
            AllowSynchronousContinuations = false
        });
    }

    /// <summary>
    ///     The subscription name.
    /// </summary>
    public string SubscriptionName => Name;

    /// <summary>
    ///     The stream the subscription reads; <see cref="KurrentDBStreams.All" /> for an all-stream subscription.
    /// </summary>
    public abstract string StreamName { get; }

    private protected abstract bool IsAllStream { get; }

    private protected abstract IEventFilter? FilterOptions { get; }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.SubscriptionDisabled(SubscriptionName);
            return;
        }

        // A stream subscription reads exactly one named stream; $all is read by an all-stream subscription.
        if (!IsAllStream)
        {
            if (string.IsNullOrWhiteSpace(StreamName))
            {
                throw new OrleansConfigurationException($"Invalid KurrentDB catch-up subscription {SubscriptionName}. {nameof(StreamName)} is required.");
            }
            if (KurrentDBStreams.IsAll(StreamName))
            {
                throw new OrleansConfigurationException($"Invalid KurrentDB catch-up subscription {SubscriptionName}. {nameof(StreamName)} must not be {KurrentDBStreams.All}; derive from KurrentDBAllStreamCatchupSubscription to read {KurrentDBStreams.All}.");
            }
        }

        SetState(KurrentDBCatchupSubscriptionState.Starting);
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receivingTask = ReceiveAsync(stopping);
        try
        {
            if (IsAllStream)
            {
                await SubscribeToAll(stopping.Token);
            }
            else
            {
                await SubscribeToStream(stopping.Token);
            }
        }
        finally
        {
            await stopping.CancelAsync();
            await _receivingTask;
            await FlushCheckpointAsync();
            if (_state != KurrentDBCatchupSubscriptionState.Faulted)
            {
                SetState(KurrentDBCatchupSubscriptionState.Stopped);
            }
        }
    }

    private void SetState(KurrentDBCatchupSubscriptionState state, string? description = null, Exception? exception = null)
    {
        if (_state == KurrentDBCatchupSubscriptionState.Faulted && state != KurrentDBCatchupSubscriptionState.Starting)
        {
            // A faulted subscription stays faulted until the worker is restarted.
            return;
        }
        if (_state == state && description is null && exception is null && _statusRegistry.TryGetStatus(SubscriptionName, out _))
        {
            return;
        }
        _state = state;
        _statusRegistry.Report(new KurrentDBCatchupSubscriptionStatus(SubscriptionName, state, description, exception));
    }

    private async Task HandleBatchAsync(List<IStreamedEvent<object>> streamEvents)
    {
        var started = Stopwatch.GetTimestamp();
        await OnNextBatchAsync(streamEvents);
        _metrics.BatchProcessed(SubscriptionName, streamEvents.Count, Stopwatch.GetElapsedTime(started));
        if (_state == KurrentDBCatchupSubscriptionState.Retrying)
        {
            SetState(KurrentDBCatchupSubscriptionState.Running);
        }
    }

    private async Task FlushCheckpointAsync()
    {
        // The monitor only tracks positions of successfully handled batches, so flushing here never checkpoints a
        // failed or unprocessed batch.
        try
        {
            await _checkpointMonitor.CloseAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.CheckpointFlushFailed(ex, SubscriptionName);
        }
    }

    private async Task<bool> DelayBeforeReconnectAsync(Exception ex, int attempt, CancellationToken cancellationToken)
    {
        var delay = KurrentDBReconnectBackoff.GetDelay(attempt, _options.RetryOptions.BaseDelay, _options.RetryOptions.MaxDelay, Random.Shared.NextDouble());
        SetState(KurrentDBCatchupSubscriptionState.Reconnecting, $"Reconnect attempt {attempt} in {delay}.", ex);
        _metrics.Reconnected(SubscriptionName);
        var level = KurrentDBBatchRetryState.GetRetryLogLevel(attempt);
        if (level is { } logLevel)
        {
            _logger.SubscriptionDropped(logLevel, ex, SubscriptionName, attempt, delay);
        }
        try
        {
            await Task.Delay(delay, cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task ReceiveAsync(CancellationTokenSource stopping)
    {
        try
        {
            var receiver = new KurrentDBCatchupBatchReceiver(_channel.Reader, _checkpointMonitor, HandleBatchAsync,
                _options.BufferOptions.MaxEventsPerProcessLoop, CreateBatchResiliencePipeline(), _logger,
                partialTransactionHandling: _options.BufferOptions.PartialTransactionHandling,
                hasMoreTransactionEventsAsync: HasMoreTransactionEventsAsync,
                groupTransactions: IsAllStream);
            await receiver.RunAsync(stopping.Token);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested)
        {
        }
        catch (KurrentDBBatchProcessingException ex)
        {
            // The failed batch was not checkpointed, so stopping here guarantees it is redelivered when the
            // subscription next starts rather than being skipped.
            var state = ex.State;
            _logger.SubscriptionStoppedAfterRetries(ex.InnerException, SubscriptionName, state.EventCount, state.FirstPosition, state.LastPosition, state.FailedAttempts, state.Elapsed);
            SetState(KurrentDBCatchupSubscriptionState.Faulted,
                $"A batch of {state.EventCount} events from position {state.FirstPosition} to {state.LastPosition} could not be processed after {state.FailedAttempts} attempts. No checkpoint was recorded for the failed batch.",
                ex.InnerException);
            _metrics.Failed(SubscriptionName);
            await stopping.CancelAsync();
        }
        catch (Exception ex)
        {
            _logger.SubscriptionStopped(ex, SubscriptionName);
            SetState(KurrentDBCatchupSubscriptionState.Faulted, "A batch could not be processed. No checkpoint was recorded for the failed batch.", ex);
            _metrics.Failed(SubscriptionName);
            await stopping.CancelAsync();
        }
    }

    private async Task<bool> HasMoreTransactionEventsAsync(Guid transactionId, ulong afterPosition, CancellationToken cancellationToken)
    {
        // Transactions are only grouped for $all. The read is inclusive of the last event seen, which is skipped.
        var read = _client.ReadAllAsync(Direction.Forwards, new Position(afterPosition, afterPosition),
            FilterOptions ?? global::KurrentDB.Client.EventTypeFilter.ExcludeSystemEvents(),
            maxCount: 2, cancellationToken: cancellationToken);
        await foreach (var resolvedEvent in read)
        {
            if (resolvedEvent.OriginalPosition?.CommitPosition is not { } position || position <= afterPosition)
            {
                continue;
            }
            return KurrentDBEventMetadataCodec.Read(resolvedEvent.Event.Metadata.Span).TransactionId == transactionId;
        }
        return false;
    }

    /// <summary>
    ///     Creates the pipeline each call to <see cref="OnNextBatchAsync" /> runs through.
    /// </summary>
    /// <returns>
    ///     By default, retries exceptions <see cref="IsTransient" /> accepts indefinitely, with exponential backoff and
    ///     jitter from <see cref="KurrentDBBatchRetryOptions.BaseDelay" /> capped at
    ///     <see cref="KurrentDBBatchRetryOptions.MaxDelay" />. Any other exception stops the subscription without
    ///     checkpointing the batch.
    /// </returns>
    protected virtual ResiliencePipeline CreateBatchResiliencePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = args => ValueTask.FromResult(args.Outcome.Exception is { } ex && IsTransient(ex)),
                MaxRetryAttempts = int.MaxValue,
                BackoffType = DelayBackoffType.Exponential,
                Delay = _options.RetryOptions.BaseDelay,
                MaxDelay = _options.RetryOptions.MaxDelay,
                UseJitter = true,
                OnRetry = args =>
                {
                    SetState(KurrentDBCatchupSubscriptionState.Retrying, $"Batch attempt {args.AttemptNumber + 1} failed. Retrying in {args.RetryDelay}.", args.Outcome.Exception);
                    _metrics.BatchRetried(SubscriptionName);
                    LogRetry(args.Context, args.Outcome.Exception, args.RetryDelay);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    private void LogRetry(ResilienceContext context, Exception? exception, TimeSpan retryDelay)
    {
        if (!context.Properties.TryGetValue(KurrentDBBatchRetryState.Key, out var state))
        {
            _logger.BatchRetrying(exception, SubscriptionName, retryDelay);
            return;
        }
        if (KurrentDBBatchRetryState.GetRetryLogLevel(state.FailedAttempts) is not { } level)
        {
            return;
        }
        _logger.BatchAttemptFailed(level, exception, SubscriptionName, state.EventCount, state.FirstPosition, state.LastPosition, state.FailedAttempts, state.Elapsed, retryDelay);
    }

    /// <summary>
    ///     Determines whether a failure processing a batch is worth retrying.
    /// </summary>
    /// <param name="exception">The exception thrown by <see cref="OnNextBatchAsync" />.</param>
    /// <returns>
    ///     By default, <see langword="true" /> for timeouts, Orleans silo, gateway and messaging failures around grain
    ///     calls, KurrentDB leader and discovery failures, and unavailable or timed out gRPC and HTTP calls.
    /// </returns>
    protected virtual bool IsTransient(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,
            SiloUnavailableException => true,
            OrleansMessageRejectionException => true,
            GatewayTooBusyException => true,
            ClientNotAvailableException => true,
            NotLeaderException => true,
            DiscoveryException => true,
            RpcException rpc => rpc.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded or StatusCode.ResourceExhausted or StatusCode.Aborted,
            HttpRequestException => true,
            AggregateException aggregate => aggregate.InnerExceptions.Count > 0 && aggregate.InnerExceptions.All(IsTransient),
            _ => exception.InnerException is { } inner && IsTransient(inner)
        };
    }

    #region Subscribe To All

    private async Task SubscribeToAll(CancellationToken cancellationToken)
    {
        var reconnectAttempt = 0;
        var checkpointLoaded = false;
        ulong? lastReceivedPosition = null;
        Subscribe:
        try
        {
            // The stored checkpoint is only read before the first connect, while nothing is buffered. Reconnects resume
            // after the last position written to the channel, because events still buffered or being handled are not
            // reflected in the checkpoint yet and would otherwise be read and handled twice.
            if (!checkpointLoaded)
            {
                lastReceivedPosition = await GetStartPositionAsync(cancellationToken);
                checkpointLoaded = true;
            }
            var startPosition = !lastReceivedPosition.HasValue ? FromAll.Start : FromAll.After(new Position(lastReceivedPosition.Value, lastReceivedPosition.Value));
            var subscriptionFilterOptions = FilterOptions != null 
                ? new SubscriptionFilterOptions(FilterOptions, _options.CheckpointMonitorOptions.KurrentDBCheckpointInterval)
                : null;

            await using var subscription = _client.SubscribeToAll(startPosition, _options.ResolveLinkTos, subscriptionFilterOptions, null, cancellationToken);

            await foreach (var message in subscription.Messages)
            {
                switch (message)
                {
                    case StreamMessage.Event(ResolvedEvent e):
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        if (e.OriginalPosition is not { } eventPosition)
                        {
                            // A $all subscription always reports a position. Skipping is safer than guessing one.
                            _logger.EventWithoutPosition(SubscriptionName, e.OriginalEvent.EventId.ToGuid());
                            break;
                        }
                        await EventAppearedFromAllStream(e, cancellationToken);
                        lastReceivedPosition = eventPosition.CommitPosition;
                        break;

                    case StreamMessage.AllStreamCheckpointReached(Position position):
                        {
                            _logger.AllStreamCheckpointReached(position, SubscriptionName);
                            var checkpointEvent = position.ToAllStreamCheckpointEvent(_options.ConnectionName);
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                            lastReceivedPosition = position.CommitPosition;
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        _logger.AllStreamSubscriptionConfirmed(SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.SubscriptionCaughtUp(SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.SubscriptionFellBehind(SubscriptionName);
                        break;

                    case StreamMessage.Unknown:
                        _logger.AllStreamUnknownMessage(SubscriptionName);
                        break;

                    default:
                        _logger.UnhandledMessageType(message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelled(ex, SubscriptionName);
        }
        catch (ObjectDisposedException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelledByUser(ex, SubscriptionName);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelled(null, SubscriptionName);
        }
        catch (UnknownEventTypeException ex)
        {
            HaltOnUnknownEventType(ex);
        }
        catch (Exception ex)
        {
            if (await DelayBeforeReconnectAsync(ex, ++reconnectAttempt, cancellationToken))
            {
                goto Subscribe;
            }
        }
    }

    private async Task EventAppearedFromAllStream(ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        var eventHasBeenDeleted = resolvedEvent.Event is null;
        if (eventHasBeenDeleted)
        {
            return;
        }
        if (!TryResolveEventType(resolvedEvent, out var eventType))
        {
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToAllStreamEvent(_options.ConnectionName, deserializedEvent, _options.StreamNameParser);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    #endregion

    #region Subscribe To Stream

    private async Task SubscribeToStream(CancellationToken cancellationToken)
    {
        var reconnectAttempt = 0;
        var checkpointLoaded = false;
        ulong? lastReceivedPosition = null;
        Subscribe:
        try
        {
            // See SubscribeToAll: reconnects resume after the last position written to the channel.
            if (!checkpointLoaded)
            {
                lastReceivedPosition = await GetStartPositionAsync(cancellationToken);
                checkpointLoaded = true;
            }
            var startPosition = !lastReceivedPosition.HasValue ? FromStream.Start : FromStream.After(new StreamPosition(lastReceivedPosition.Value));

            await using var subscription = _client.SubscribeToStream(StreamName, startPosition, _options.ResolveLinkTos, null, cancellationToken);

            await foreach (var message in subscription.Messages)
            {
                switch (message)
                {
                    case StreamMessage.Event(var e):
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        await EventAppearedFromStream(e, cancellationToken);
                        lastReceivedPosition = e.OriginalEventNumber.ToUInt64();
                        break;

                    case StreamMessage.StreamCheckpointReached(StreamPosition position):
                        {
                            _logger.StreamCheckpointReached(position, SubscriptionName);
                            var checkpointEvent = position.ToStreamCheckpointEvent(_options.ConnectionName);
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                            lastReceivedPosition = position.ToUInt64();
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        _logger.StreamSubscriptionConfirmed(SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.SubscriptionCaughtUp(SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.SubscriptionFellBehind(SubscriptionName);
                        break;

                    case StreamMessage.Unknown:
                        _logger.StreamUnknownMessage(SubscriptionName);
                        break;

                    default:
                        _logger.UnhandledMessageType(message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelled(ex, SubscriptionName);
        }
        catch (ObjectDisposedException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelledByUser(ex, SubscriptionName);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            _logger.SubscriptionCancelled(null, SubscriptionName);
        }
        catch (UnknownEventTypeException ex)
        {
            HaltOnUnknownEventType(ex);
        }
        catch (Exception ex)
        {
            if (await DelayBeforeReconnectAsync(ex, ++reconnectAttempt, cancellationToken))
            {
                goto Subscribe;
            }
        }
    }

    private async Task EventAppearedFromStream(ResolvedEvent resolvedEvent, CancellationToken cancellationToken)
    {
        var eventHasBeenDeleted = resolvedEvent.Event is null;
        if (eventHasBeenDeleted)
        {
            return;
        }
        if (!TryResolveEventType(resolvedEvent, out var eventType))
        {
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToStreamEvent(_options.ConnectionName, deserializedEvent, _options.StreamNameParser);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    private bool TryDeserialize(ResolvedEvent resolvedEvent, Type eventType, [NotNullWhen(true)] out object? deserializedEvent)
    {
        var record = resolvedEvent.Event;
        return KurrentDBCatchupEventDeserializer.TryDeserialize(_storageSerializer, _logger, record.EventType, record.EventStreamId, record.Data, eventType, SubscriptionName, out deserializedEvent);
    }

    #endregion

    /// <summary>
    ///     Resolves where to start reading: the stored checkpoint, otherwise the current end when
    ///     <see cref="KurrentDBCatchupSubscriptionOptions.StartFromNow" /> is set, otherwise the beginning
    ///     (<see langword="null" />). For <c>$all</c> this is a commit position; for a single stream a stream revision.
    /// </summary>
    private async Task<ulong?> GetStartPositionAsync(CancellationToken cancellationToken)
    {
        var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync(cancellationToken);
        if (lastCheckpoint == KurrentDBCheckpointMonitor.StreamStartPosition && !IsAllStream)
        {
            _logger.StartingFromBeginning(SubscriptionName);
            return null;
        }
        if (lastCheckpoint.HasValue)
        {
            _logger.ResumingFromCheckpoint(SubscriptionName, lastCheckpoint.Value);
            return lastCheckpoint.Value;
        }
        if (_options.StartFromNow && await ReadLastPositionAsync(cancellationToken) is { } lastPosition)
        {
            // Starting from the current end is turned into a real position and committed right away. Subscribing from
            // the end instead would pick a new end on every reconnect or restart before the first checkpoint,
            // skipping whatever was written in between.
            _logger.StartingFromLatest(SubscriptionName, lastPosition);
            await _checkpointMonitor.CommitInitialCheckpointAsync(lastPosition, cancellationToken);
            return lastPosition;
        }
        // An empty database or missing stream has nothing to skip, so starting from now and from the beginning are the same.
        _logger.StartingFromBeginning(SubscriptionName);
        if (_options.StartFromNow && !IsAllStream)
        {
            // A missing stream is committed as starting before its first event, so a restart before the first regular
            // commit does not mistake the events written meanwhile for history to skip.
            await _checkpointMonitor.CommitInitialCheckpointAsync(KurrentDBCheckpointMonitor.StreamStartPosition, cancellationToken);
        }
        return null;
    }

    private async Task<ulong?> ReadLastPositionAsync(CancellationToken cancellationToken)
    {
        if (IsAllStream)
        {
            await foreach (var resolvedEvent in _client.ReadAllAsync(Direction.Backwards, Position.End, maxCount: 1, resolveLinkTos: false, cancellationToken: cancellationToken))
            {
                return resolvedEvent.OriginalPosition?.CommitPosition;
            }
            return null;
        }
        var readResult = _client.ReadStreamAsync(Direction.Backwards, StreamName, StreamPosition.End, maxCount: 1, resolveLinkTos: false, cancellationToken: cancellationToken);
        if (await readResult.ReadState == ReadState.StreamNotFound)
        {
            return null;
        }
        await foreach (var resolvedEvent in readResult)
        {
            return resolvedEvent.OriginalEventNumber.ToUInt64();
        }
        return null;
    }

    private bool TryResolveEventType(ResolvedEvent resolvedEvent, [NotNullWhen(true)] out Type? eventType)
    {
        var record = resolvedEvent.Event;
        if (_typeMapper.TryGetType(record.EventType, out eventType))
        {
            return true;
        }
        if (_options.UnknownEventTypeBehavior == UnknownEventTypeBehavior.Skip)
        {
            _logger.EventTypeNotMappedSkipped(record.EventType, record.EventStreamId, SubscriptionName);
            return false;
        }
        // Throwing before the event reaches the channel keeps the checkpoint from advancing past it.
        throw new UnknownEventTypeException(record.EventType, record.EventStreamId, resolvedEvent.OriginalEvent.Position.CommitPosition, record.EventNumber.ToUInt64());
    }

    private void HaltOnUnknownEventType(UnknownEventTypeException ex)
    {
        _logger.EventTypeNotMappedHalted(ex.EventType, ex.EventStreamId, SubscriptionName);
        SetState(KurrentDBCatchupSubscriptionState.Faulted,
            $"Event type '{ex.EventType}' from stream '{ex.EventStreamId}' (revision {ex.StreamRevision}, commit position {ex.CommitPosition}) is not mapped and {nameof(KurrentDBCatchupSubscriptionOptions.UnknownEventTypeBehavior)} is {UnknownEventTypeBehavior.Halt}. No checkpoint was recorded past the event.",
            ex);
        _metrics.Failed(SubscriptionName);
    }

    private sealed class UnknownEventTypeException(string eventType, string eventStreamId, ulong commitPosition, ulong streamRevision)
        : Exception($"Event type '{eventType}' from stream '{eventStreamId}' is not mapped.")
    {
        public string EventType { get; } = eventType;
        public string EventStreamId { get; } = eventStreamId;
        public ulong CommitPosition { get; } = commitPosition;
        public ulong StreamRevision { get; } = streamRevision;
    }

    /// <summary>
    ///     Processes a batch of events read from the stream.
    /// </summary>
    /// <param name="streamEvents">The events, in stream order.</param>
    /// <remarks>
    ///     <para>
    ///         Events read from <c>$all</c> carry a cursor scoped to the whole log: topic <c>$all</c>, partition
    ///         <c>0</c>, and the commit position. Projections skip any event at or below the highest position they
    ///         have applied, so implementations must follow these rules:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             Deliver to each grain in the order received. Await each call before sending the next, or send the
    ///             grain's events as one ordered list; never call the same grain in parallel.
    ///         </item>
    ///         <item>
    ///             Only one self-hosted subscription may call a given grain. A grain fed by two subscriptions would see
    ///             one subscription's positions as already applied and skip the other's events. A subscription reads
    ///             <c>$all</c>, so the one feeding a grain should subscribe to every event that grain needs.
    ///         </item>
    ///     </list>
    /// </remarks>
    protected abstract Task OnNextBatchAsync(List<IStreamedEvent<object>> streamEvents);
}

internal static partial class KurrentDBCatchupSubscriptionLog
{
	[LoggerMessage(Level = LogLevel.Warning, Message = "Subscription '{SubscriptionName}' is disabled.")]
	public static partial void SubscriptionDisabled(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Subscription '{SubscriptionName}' failed to flush its checkpoint on shutdown. Already processed events may be redelivered.")]
	public static partial void CheckpointFlushFailed(this ILogger logger, Exception exception, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Subscription '{SubscriptionName}' stopped because a batch of {EventCount} events from position {FirstPosition} to {LastPosition} could not be processed after {FailedAttempts} attempts over {Elapsed}. No checkpoint was recorded for the failed batch.")]
	public static partial void SubscriptionStoppedAfterRetries(this ILogger logger, Exception? exception, string subscriptionName, int eventCount, ulong firstPosition, ulong lastPosition, int failedAttempts, TimeSpan elapsed);

	[LoggerMessage(Level = LogLevel.Critical, Message = "Subscription '{SubscriptionName}' stopped because a batch could not be processed. No checkpoint was recorded for the failed batch.")]
	public static partial void SubscriptionStopped(this ILogger logger, Exception exception, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Subscription '{SubscriptionName}' failed to process a batch. Retrying in {Delay}.")]
	public static partial void BatchRetrying(this ILogger logger, Exception? exception, string subscriptionName, TimeSpan delay);

	[LoggerMessage(Message = "Subscription '{SubscriptionName}' failed to process a batch of {EventCount} events from position {FirstPosition} to {LastPosition}. Attempt {FailedAttempts} failed, {Elapsed} since the first failure. Retrying in {Delay}.")]
	public static partial void BatchAttemptFailed(this ILogger logger, LogLevel level, Exception? exception, string subscriptionName, int eventCount, ulong firstPosition, ulong lastPosition, int failedAttempts, TimeSpan elapsed, TimeSpan delay);

	[LoggerMessage(Message = "Subscription '{SubscriptionName}' was dropped. Reconnect attempt {Attempt} in {Delay}.")]
	public static partial void SubscriptionDropped(this ILogger logger, LogLevel level, Exception exception, string subscriptionName, int attempt, TimeSpan delay);

	[LoggerMessage(Level = LogLevel.Information, Message = "Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.")]
	public static partial void AllStreamCheckpointReached(this ILogger logger, Position position, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Information, Message = "Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.")]
	public static partial void StreamCheckpointReached(this ILogger logger, StreamPosition position, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.")]
	public static partial void AllStreamSubscriptionConfirmed(this ILogger logger, string subscriptionName, FromAll position);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.")]
	public static partial void StreamSubscriptionConfirmed(this ILogger logger, string subscriptionName, FromStream position);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' has caught up to the head of the stream.")]
	public static partial void SubscriptionCaughtUp(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Subscription '{SubscriptionName}' has fallen behind. This means the subscription is processing events slower than they are being produced. Consider scaling up the processing or increasing the buffer size.")]
	public static partial void SubscriptionFellBehind(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Error, Message = "Received unknown message type in subscription '{SubscriptionName}'.")]
	public static partial void AllStreamUnknownMessage(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Received unknown message type in subscription '{SubscriptionName}'.")]
	public static partial void StreamUnknownMessage(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Received unhandled message type '{MessageType}' in subscription '{SubscriptionName}'.")]
	public static partial void UnhandledMessageType(this ILogger logger, string messageType, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' was cancelled.")]
	public static partial void SubscriptionCancelled(this ILogger logger, Exception? exception, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' was cancelled by the user.")]
	public static partial void SubscriptionCancelledByUser(this ILogger logger, Exception exception, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'. The event was skipped because UnknownEventTypeBehavior is Skip.")]
	public static partial void EventTypeNotMappedSkipped(this ILogger logger, string eventType, string streamName, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Error, Message = "Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'. The subscription is halted because UnknownEventTypeBehavior is Halt.")]
	public static partial void EventTypeNotMappedHalted(this ILogger logger, string eventType, string streamName, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' is resuming from checkpoint {Position}.")]
	public static partial void ResumingFromCheckpoint(this ILogger logger, string subscriptionName, ulong position);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' has no checkpoint and is starting from the latest position {Position}.")]
	public static partial void StartingFromLatest(this ILogger logger, string subscriptionName, ulong position);

	[LoggerMessage(Level = LogLevel.Information, Message = "Subscription '{SubscriptionName}' is starting from the beginning.")]
	public static partial void StartingFromBeginning(this ILogger logger, string subscriptionName);

	[LoggerMessage(Level = LogLevel.Warning, Message = "Subscription '{SubscriptionName}' skipped event {EventId} because it has no $all position.")]
	public static partial void EventWithoutPosition(this ILogger logger, string subscriptionName, Guid eventId);
}
