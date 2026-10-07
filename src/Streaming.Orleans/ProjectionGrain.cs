using Microsoft.Extensions.Logging;

using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// Grain contract used by catch-up subscriptions that run outside the Orleans stream runtime.
/// </summary>
public interface IProjectionGrain : IGrainWithStringKey
{
    /// <summary>
    /// Applies a batch of events in order.
    /// </summary>
    Task OnNextBatchAsync([Immutable] List<IStreamedEvent<object>> streamedEvents);
}

/// <summary>
/// Base class for a grain that maintains a single persistent projection state.
/// </summary>
/// <remarks>
/// <para>
///     Events reach the projection either through an Orleans stream provider, one at a time, or through
///     <see cref="IProjectionGrain.OnNextBatchAsync"/> from a catch-up subscription that runs outside the stream
///     runtime. Both paths apply events through the same <see cref="ProjectionState{TState}"/>, so they share one
///     de-duplication watermark and one write per delivery.
/// </para>
/// <para>
///     The grain holds exactly one persistent state. Prefer a separate grain per read model over fanning a delivery
///     out across several states.
/// </para>
/// </remarks>
public abstract class ProjectionGrain<TGrain, TState> : Grain, IProjectionGrain, IStreamSubscriptionObserver
    where TGrain : ProjectionGrain<TGrain, TState>
    where TState : class, IProjectionState, new()
{
    private StreamSubscriptionManager<TGrain>? _subscriptionManager;

    /// <summary>
    /// Initializes the projection with the storage for its state and the logger to report failures to.
    /// </summary>
    /// <remarks>
    /// Declare <c>[PersistentState]</c> on the derived grain's constructor parameter and pass it through; Orleans
    /// reads the attribute from the concrete grain, so the derived grain still chooses the state name and provider.
    /// </remarks>
    protected ProjectionGrain(IPersistentState<TState> storage, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(logger);

        Storage = storage;
        Logger = logger;
    }

    /// <summary>
    /// The storage that persists this projection's state.
    /// </summary>
    protected IPersistentState<TState> Storage { get; }

    /// <summary>
    /// The projection state, as held by <see cref="Storage"/>.
    /// </summary>
    protected TState State => Storage.State;

    /// <summary>
    /// Logger used to report subscription and delivery failures.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// The streams this grain subscribes to itself through an Orleans stream provider.
    /// </summary>
    /// <remarks>
    /// Empty by default, which is the correct behaviour for a purely implicit subscriber or for a projection fed
    /// only by a catch-up subscription. Override to consume one or more streams explicitly, optionally alongside
    /// implicit subscriptions.
    /// </remarks>
    protected virtual IEnumerable<StreamSubscriptionSource> Subscriptions => [];

    private StreamSubscriptionManager<TGrain> SubscriptionManager
        => _subscriptionManager ??= new StreamSubscriptionManager<TGrain>(
            this,
            Logger,
            OnNextAsync,
            OnErrorAsync,
            OnCompletedAsync);

    /// <summary>
    /// Builds a <see cref="StreamId"/> whose key is this grain's primary key.
    /// </summary>
    /// <remarks>
    /// This is the convention used by implicit subscriptions, where the runtime activates the grain whose key
    /// matches the stream key.
    /// </remarks>
    protected StreamId StreamIdFromKey(string streamNamespace)
        => StreamId.Create(streamNamespace, this.GetPrimaryKeyString());

    /// <inheritdoc/>
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
        await SubscriptionManager.SubscribeAllAsync(Subscriptions.Concat(State.Subscriptions), GetResumeTokenAsync);
    }

    /// <summary>
    /// Attaches to the subscription the runtime created for an implicit subscriber.
    /// </summary>
    public Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
        => SubscriptionManager.AttachImplicitAsync(handleFactory);

    /// <summary>
    /// Applies a batch delivered by a catch-up subscription and writes the state once if it changed.
    /// </summary>
    public Task OnNextBatchAsync([Immutable] List<IStreamedEvent<object>> streamedEvents)
    {
        if (streamedEvents is null || streamedEvents.Count == 0)
        {
            return Task.CompletedTask;
        }

        return ApplyAsync(streamedEvents);
    }

    /// <summary>
    /// Applies a single event delivered by the stream provider.
    /// </summary>
    /// <remarks>
    /// Throwing from this method is meaningful: it prevents the delivered event from being treated as
    /// successfully consumed, which is what allows a provider to avoid checkpointing past it.
    /// </remarks>
    protected virtual Task OnNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? token)
        => ApplyAsync([streamedEvent]);

    /// <summary>
    /// Invoked when the stream reports an error.
    /// </summary>
    /// <remarks>
    /// The default implementation logs and rethrows so that failures surface rather than being silently skipped.
    /// Orleans' observer contract supplies only the exception, so the streams currently held are logged instead.
    /// </remarks>
    protected virtual Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(
            exception,
            "Stream subscription faulted for {Grain}; currently subscribed to {Streams}.",
            typeof(TGrain).Name,
            SubscriptionManager.DescribeSubscriptions());

        return Task.FromException(exception);
    }

    /// <summary>
    /// Invoked when the stream completes. The default implementation does nothing.
    /// </summary>
    protected virtual Task OnCompletedAsync() => Task.CompletedTask;

    /// <summary>
    /// Returns the sequence token to resume delivery from, or <see langword="null"/> to use the
    /// provider default. Override to resume from a durably recorded position.
    /// </summary>
    protected virtual Task<StreamSequenceToken?> GetResumeTokenAsync(StreamSubscriptionSource source)
        => Task.FromResult<StreamSequenceToken?>(null);

    /// <summary>
    /// Ends this grain's explicit subscriptions and clears the streams recorded in the projection state.
    /// </summary>
    /// <remarks>
    /// Deliberately not called on deactivation. An Orleans subscription is durable and outlives the activation
    /// that created it; call this only when the grain is logically finished with the stream.
    /// </remarks>
    protected async Task UnsubscribeAsync()
    {
        await SubscriptionManager.UnsubscribeAsync();
        if (State.ClearSubscriptions())
        {
            await Storage.WriteStateAsync();
        }
    }

    /// <summary>
    /// Subscribes to a single stream at runtime, or resumes the existing subscription to it.
    /// </summary>
    /// <remarks>
    /// The stream is recorded in the projection state and written, so the subscription is resumed when the grain
    /// reactivates. Use <see cref="Subscriptions"/> instead for streams the grain always consumes.
    /// </remarks>
    protected async Task SubscribeAsync(StreamSubscriptionSource source)
    {
        await SubscriptionManager.SubscribeAsync(source, GetResumeTokenAsync);
        if (State.AddSubscription(source))
        {
            await Storage.WriteStateAsync();
        }
    }

    /// <summary>
    /// Ends this grain's explicit subscription to a single stream.
    /// </summary>
    /// <remarks>
    /// Removes the stream from the projection state so a later activation does not resume it. A stream returned by
    /// <see cref="Subscriptions"/> is subscribed again on the next activation regardless.
    /// </remarks>
    protected async Task UnsubscribeAsync(StreamSubscriptionSource source)
    {
        await SubscriptionManager.UnsubscribeAsync(source);
        if (State.RemoveSubscription(source))
        {
            await Storage.WriteStateAsync();
        }
    }

    /// <summary>
    /// Whether the runtime supplies subscriptions for the given stream because of an implicit subscription.
    /// </summary>
    protected static bool IsImplicit(StreamId streamId) => StreamSubscriptionManager<TGrain>.IsImplicit(streamId);

    /// <summary>
    /// The single apply path shared by stream and batch delivery.
    /// </summary>
    /// <remarks>
    /// The state is written before delivery is acknowledged, so a failure here propagates and the events are not
    /// treated as consumed. The sequence watermark is only durable once the write succeeds, which is what makes
    /// redelivery safe.
    /// </remarks>
    private async Task ApplyAsync(IReadOnlyList<IStreamedEvent<object>> streamedEvents)
    {
        var hasChanges = await State.WhenAsync(streamedEvents);
        if (hasChanges)
        {
            await Storage.WriteStateAsync();
        }
    }
}
