using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

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
public record KurrentDBCatchupSubscriptionInfo(string Name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)][property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type Type);

/// <summary>
///     A hosted catch-up subscription that reads a KurrentDB stream and delivers events in batches.
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
    /// <summary>
    ///     The registration info for this subscription.
    /// </summary>
    public static KurrentDBCatchupSubscriptionInfo Info => new(Name, Type);

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
    public KurrentDBCatchupSubscription(IServiceProvider serviceProvider)
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
    ///     The stream the subscription reads, or <see cref="KurrentDBStreams.All" /> to read every stream.
    /// </summary>
    public abstract string StreamName { get; }

    /// <summary>
    ///     The server-side filter applied when reading <c>$all</c>, or <see langword="null" /> for none.
    /// </summary>
    public abstract IEventFilter? FilterOptions { get; }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Subscription '{SubscriptionName}' is disabled.", SubscriptionName);
            return;
        }

        SetState(KurrentDBCatchupSubscriptionState.Starting);
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receivingTask = ReceiveAsync(stopping);
        try
        {
            if (KurrentDBStreams.IsAll(StreamName))
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
            _logger.LogWarning(ex, "Subscription '{SubscriptionName}' failed to flush its checkpoint on shutdown. Already processed events may be redelivered.", SubscriptionName);
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
            _logger.Log(logLevel, ex, "Subscription '{SubscriptionName}' was dropped. Reconnect attempt {Attempt} in {Delay}.", SubscriptionName, attempt, delay);
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
                _options.BufferOptions.MaxEventsPerProcessLoop, CreateBatchResiliencePipeline(), _logger);
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
            _logger.LogCritical(ex.InnerException, "Subscription '{SubscriptionName}' stopped because a batch of {EventCount} events from position {FirstPosition} to {LastPosition} could not be processed after {FailedAttempts} attempts over {Elapsed}. No checkpoint was recorded for the failed batch.",
                SubscriptionName, state.EventCount, state.FirstPosition, state.LastPosition, state.FailedAttempts, state.Elapsed);
            SetState(KurrentDBCatchupSubscriptionState.Faulted,
                $"A batch of {state.EventCount} events from position {state.FirstPosition} to {state.LastPosition} could not be processed after {state.FailedAttempts} attempts. No checkpoint was recorded for the failed batch.",
                ex.InnerException);
            _metrics.Failed(SubscriptionName);
            await stopping.CancelAsync();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Subscription '{SubscriptionName}' stopped because a batch could not be processed. No checkpoint was recorded for the failed batch.", SubscriptionName);
            SetState(KurrentDBCatchupSubscriptionState.Faulted, "A batch could not be processed. No checkpoint was recorded for the failed batch.", ex);
            _metrics.Failed(SubscriptionName);
            await stopping.CancelAsync();
        }
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
            _logger.LogWarning(exception, "Subscription '{SubscriptionName}' failed to process a batch. Retrying in {Delay}.", SubscriptionName, retryDelay);
            return;
        }
        if (KurrentDBBatchRetryState.GetRetryLogLevel(state.FailedAttempts) is not { } level)
        {
            return;
        }
        _logger.Log(level, exception, "Subscription '{SubscriptionName}' failed to process a batch of {EventCount} events from position {FirstPosition} to {LastPosition}. Attempt {FailedAttempts} failed, {Elapsed} since the first failure. Retrying in {Delay}.",
            SubscriptionName, state.EventCount, state.FirstPosition, state.LastPosition, state.FailedAttempts, state.Elapsed, retryDelay);
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
        Subscribe:
        try
        {
            var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync();
            var startPosition = !lastCheckpoint.HasValue ? FromAll.Start : FromAll.After(new Position(lastCheckpoint.Value, lastCheckpoint.Value));
            var subscriptionFilterOptions = FilterOptions != null 
                ? new SubscriptionFilterOptions(FilterOptions, _options.CheckpointMonitorOptions.KurrentDBCheckpointInterval)
                : null;

            await using var subscription = _client.SubscribeToAll(startPosition, false, subscriptionFilterOptions, null, cancellationToken);

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
                        await EventAppearedFromAllStream(e, cancellationToken);
                        break;

                    case StreamMessage.AllStreamCheckpointReached(Position position):
                        {
                            _logger.LogInformation("Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.", position, SubscriptionName);
                            var checkpointEvent = position.ToAllStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        _logger.LogInformation("Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.", SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.LogInformation("Subscription '{SubscriptionName}' has caught up to the head of the stream.", SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.LogWarning("Subscription '{SubscriptionName}' has fallen behind. This means the subscription is processing events slower than they are being produced. Consider scaling up the processing or increasing the buffer size.", SubscriptionName);
                        break;

                    case StreamMessage.LastAllStreamPosition(var position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached last position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToAllStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.Unknown:
                        _logger.LogError("Received unknown message type in subscription '{SubscriptionName}'.", SubscriptionName);
                        break;

                    default:
                        _logger.LogWarning("Received unhandled message type '{MessageType}' in subscription '{SubscriptionName}'.", message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled by the user.", SubscriptionName);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
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
        var eventTypeNotMapped = !_typeMapper.TryGetType(resolvedEvent.Event!.EventType, out var eventType);
        if (eventTypeNotMapped)
        {
            _logger.LogWarning("Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'.",
                resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToAllStreamEvent(deserializedEvent, _options.StreamNameParser);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    #endregion

    #region Subscribe To Stream

    private async Task SubscribeToStream(CancellationToken cancellationToken)
    {
        var reconnectAttempt = 0;
        Subscribe:
        try
        {
            var lastCheckpoint = await _checkpointMonitor.GetLastCheckpointAsync();
            var startPosition = !lastCheckpoint.HasValue ? FromStream.Start : FromStream.After(new StreamPosition(lastCheckpoint.Value));

            await using var subscription = _client.SubscribeToStream(StreamName, startPosition, false, null, cancellationToken);

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
                        break;

                    case StreamMessage.StreamCheckpointReached(StreamPosition position):
                        {
                            _logger.LogInformation("Checkpoint reached at position {Position} for subscription '{SubscriptionName}'.", position, SubscriptionName);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.SubscriptionConfirmation:
                        reconnectAttempt = 0;
                        if (_state is KurrentDBCatchupSubscriptionState.Starting or KurrentDBCatchupSubscriptionState.Reconnecting)
                        {
                            SetState(KurrentDBCatchupSubscriptionState.Running);
                        }
                        _logger.LogInformation("Subscription '{SubscriptionName}' confirmed. Starting from position {Position}.", SubscriptionName, startPosition);
                        break;

                    case StreamMessage.CaughtUp:
                        _logger.LogInformation("Subscription '{SubscriptionName}' has caught up to the head of the stream.", SubscriptionName);
                        break;

                    case StreamMessage.FellBehind:
                        _logger.LogWarning("Subscription '{SubscriptionName}' has fallen behind. This means the subscription is processing events slower than they are being produced. Consider scaling up the processing or increasing the buffer size.", SubscriptionName);
                        break;

                    case StreamMessage.FirstStreamPosition(StreamPosition position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached first position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.LastStreamPosition(StreamPosition position):
                        {
                            _logger.LogInformation("Subscription '{SubscriptionName}' reached last position of the stream at {Position}.", SubscriptionName, position);
                            var checkpointEvent = position.ToStreamCheckpointEvent();
                            await _channel.Writer.WriteAsync(checkpointEvent, cancellationToken);
                        }
                        break;

                    case StreamMessage.Unknown:
                        _logger.LogWarning("Received unknown message type in subscription '{SubscriptionName}'.", SubscriptionName);
                        break;

                    default:
                        _logger.LogWarning("Received unhandled message type '{MessageType}' in subscription '{SubscriptionName}'.", message.GetType().Name, SubscriptionName);
                        break;
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogInformation(ex, "Subscription '{SubscriptionName}' was cancelled by the user.", SubscriptionName);
        }
        catch (Exception) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Subscription '{SubscriptionName}' was cancelled.", SubscriptionName);
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
        var eventTypeNotMapped = !_typeMapper.TryGetType(resolvedEvent.Event!.EventType, out var eventType);
        if (eventTypeNotMapped)
        {
            _logger.LogWarning("Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'.",
                resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
        {
            return;
        }

        var streamEvent = resolvedEvent.ToStreamEvent(deserializedEvent, _options.StreamNameParser);

        await _channel.Writer.WriteAsync(streamEvent, cancellationToken);
    }

    private bool TryDeserialize(ResolvedEvent resolvedEvent, Type eventType, [NotNullWhen(true)] out object? deserializedEvent)
    {
        var record = resolvedEvent.Event;
        return KurrentDBCatchupEventDeserializer.TryDeserialize(_storageSerializer, _logger, record.EventType, record.EventStreamId, record.Data, eventType, SubscriptionName, out deserializedEvent);
    }

    #endregion

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
