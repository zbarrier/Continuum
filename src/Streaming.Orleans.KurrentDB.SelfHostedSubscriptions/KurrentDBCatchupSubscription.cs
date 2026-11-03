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
            _logger.SubscriptionDisabled(SubscriptionName);
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
                                incompleteTransactionTimeout: _options.BufferOptions.IncompleteTransactionTimeout);
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
                lastReceivedPosition = await _checkpointMonitor.GetLastCheckpointAsync(cancellationToken);
                checkpointLoaded = true;
            }
            var startPosition = !lastReceivedPosition.HasValue ? FromAll.Start : FromAll.After(new Position(lastReceivedPosition.Value, lastReceivedPosition.Value));
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
                        if (e.OriginalPosition is { } eventPosition)
                        {
                            lastReceivedPosition = eventPosition.CommitPosition;
                        }
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
            _logger.EventTypeNotMapped(resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
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
                lastReceivedPosition = await _checkpointMonitor.GetLastCheckpointAsync(cancellationToken);
                checkpointLoaded = true;
            }
            var startPosition = !lastReceivedPosition.HasValue ? FromStream.Start : FromStream.After(new StreamPosition(lastReceivedPosition.Value));

            await using var subscription = _client.SubscribeToStream(StreamName, startPosition, true, null, cancellationToken);

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
            _logger.EventTypeNotMapped(resolvedEvent.Event.EventType, StreamName, SubscriptionName);
            return;
        }
        if (!TryDeserialize(resolvedEvent, eventType!, out var deserializedEvent))
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

	[LoggerMessage(Level = LogLevel.Warning, Message = "Failed to map event type '{EventType}' from stream '{StreamName}' in subscription '{SubscriptionName}'.")]
	public static partial void EventTypeNotMapped(this ILogger logger, string eventType, string streamName, string subscriptionName);
}
