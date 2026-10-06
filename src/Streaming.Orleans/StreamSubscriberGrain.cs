using Microsoft.Extensions.Logging;

using Orleans.Streams;
using Orleans.Streams.Core;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// Base class for grains that consume events from an Orleans stream provider one event at a time.
/// </summary>
/// <remarks>
/// This abstraction is intentionally provider agnostic: it depends only on the Orleans streaming
/// abstractions and never on a specific provider implementation. It is also deliberately separate
/// from <see cref="ProjectionGrain{TGrain}"/>, which exists to absorb catch-up volume by accepting a
/// batch and fanning it out across multiple persistent states. Stream delivery is single-event, so
/// that fan-out buys no write amortization here; prefer one meaningful grain per persistent state.
///
/// Consuming through an Orleans stream provider constrains which providers can feed this grain.
/// Orleans orders stream cursors by the <see cref="long"/> in its <c>EventSequenceToken</c>, so a
/// provider whose sequence numbers exceed that range, such as Kinesis, cannot be delivered here and
/// must be consumed through a catch-up subscription instead.
/// </remarks>
public abstract class StreamSubscriberGrain<TGrain> : Grain, IStreamSubscriptionObserver
    where TGrain : StreamSubscriberGrain<TGrain>
{
    /// <summary>
    /// Namespace predicates declared by <c>[ImplicitStreamSubscription]</c> on <typeparamref name="TGrain"/>.
    /// </summary>
    /// <remarks>
    /// Static so the reflection runs once per closed generic type rather than on every activation. An instance
    /// initializer compiles into the constructor, so the equivalent instance field would repeat this work for
    /// every grain the silo activates. Because the type is generic over <typeparamref name="TGrain"/>, each grain
    /// type gets its own initializer run, which is exactly the required granularity.
    /// </remarks>
    private static readonly IStreamNamespacePredicate[] ImplicitPredicates =
        typeof(TGrain)
            .GetCustomAttributes(typeof(ImplicitStreamSubscriptionAttribute), inherit: true)
            .Cast<ImplicitStreamSubscriptionAttribute>()
            .Select(attribute => attribute.Predicate)
            .ToArray();

    private readonly Dictionary<Guid, Subscription> _subscriptions = [];

    /// <summary>
    /// A subscription handle together with how it was established.
    /// </summary>
    /// <remarks>
    /// The origin is recorded where it is known rather than inferred later: the runtime only calls
    /// <see cref="OnSubscribed"/> for subscriptions it created, and everything produced by <c>SubscribeAsync</c>
    /// is by definition explicit. A single store with this flag is enough because every decision that depends on
    /// the distinction is made per handle rather than over the set as a whole.
    /// </remarks>
    private readonly record struct Subscription(
        StreamSubscriptionHandle<IStreamedEvent<object>> Handle,
        bool IsImplicit);

    /// <summary>
    /// The streams this grain subscribes to itself.
    /// </summary>
    /// <remarks>
    /// Empty by default, which is the correct behaviour for a purely implicit subscriber: the runtime chooses both
    /// the provider and the stream, so there is nothing for the grain to declare. Override to consume one or more
    /// streams explicitly, optionally alongside implicit subscriptions.
    /// </remarks>
    protected virtual IEnumerable<StreamSubscriptionSource> Subscriptions => [];

    /// <summary>
    /// Logger used to report subscription and delivery failures.
    /// </summary>
    protected abstract ILogger Logger { get; }

    /// <summary>
    /// Builds a <see cref="StreamId"/> whose key is this grain's primary key.
    /// </summary>
    /// <remarks>
    /// This is the convention used by implicit subscriptions, where the runtime activates the grain whose
    /// key matches the stream key. Implicit subscribers override <see cref="StreamId"/> with this helper
    /// and add <c>[ImplicitStreamSubscription]</c>; no separate base class is required.
    /// </remarks>
    protected StreamId StreamIdFromKey(string streamNamespace)
        => StreamId.Create(streamNamespace, this.GetPrimaryKeyString());

    /// <summary>
    /// Handles a single event delivered by the stream provider.
    /// </summary>
    /// <remarks>
    /// Throwing from this method is meaningful: it prevents the delivered event from being treated as
    /// successfully consumed, which is what allows a provider to avoid checkpointing past it.
    /// </remarks>
    protected abstract Task OnNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? token);

    /// <summary>
    /// Invoked when the stream reports an error.
    /// </summary>
    /// <remarks>
    /// The default implementation logs and rethrows so that failures surface rather than being
    /// silently skipped. Override to opt into a skip policy.
    ///
    /// Orleans' observer contract supplies only the exception, so the faulting subscription cannot be identified
    /// from the callback. That is rarely a loss: a fault raised by <see cref="OnNextAsync"/> carries its own stack
    /// and the event being handled, while a fault raised by the provider is usually transport wide and affects
    /// every stream on it. The streams currently held are logged instead, which frames the failure correctly in
    /// both cases without narrowing the contract to something Orleans cannot honour.
    /// </remarks>
    protected virtual Task OnErrorAsync(Exception exception)
    {
        Logger.LogError(
            exception,
            "Stream subscription faulted for {Grain}; currently subscribed to {Streams}.",
            typeof(TGrain).Name,
            SubscribedStreams());

        return Task.FromException(exception);
    }

    /// <summary>
    /// Describes the subscriptions this grain currently holds, for diagnostics.
    /// </summary>
    private string SubscribedStreams()
        => _subscriptions.Count == 0
            ? "(none)"
            : string.Join(
                ", ",
                _subscriptions.Values.Select(subscription => string.Concat(
                    subscription.Handle.ProviderName,
                    "/",
                    subscription.Handle.StreamId.ToString(),
                    subscription.IsImplicit ? " (implicit)" : " (explicit)")));

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

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        foreach (var source in Subscriptions)
        {
            // An implicit subscription activates the grain because an event arrived, and the runtime then calls
            // OnSubscribed with a handle already positioned at that event. Subscribing here as well would attach a
            // second handle at the current position, so streams the runtime covers are skipped. The check is per
            // stream rather than per grain so that a grain may combine implicit and explicit subscriptions.
            if (IsImplicit(source.StreamId))
            {
                continue;
            }

            await SubscribeAsync(source);
        }
    }

    /// <summary>
    /// Attaches to the subscription the runtime created for an implicit subscriber.
    /// </summary>
    /// <remarks>
    /// The handle carries the sequence token of the event that triggered the activation, so attaching to it
    /// delivers that event. Subscribing independently would start from the current position and lose it.
    /// </remarks>
    public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
    {
        var handle = handleFactory.Create<IStreamedEvent<object>>();
        var resumed = await handle.ResumeAsync(HandleNextAsync, HandleErrorAsync, HandleCompletedAsync);

        // Keyed by handle id because the runtime calls this once per implicit subscription, and two handles may
        // refer to the same stream. Keying by stream would let the second call discard the first handle, which
        // would leak a live subscription that could no longer be unsubscribed.
        _subscriptions[resumed.HandleId] = new Subscription(resumed, IsImplicit: true);
    }

    /// <summary>
    /// Whether the runtime supplies subscriptions for the given stream because of an implicit subscription.
    /// </summary>
    /// <remarks>
    /// Determined from the attributes rather than from a flag set in <see cref="OnSubscribed" />, because the
    /// runtime calls <c>OnActivateAsync</c> before <c>OnSubscribed</c>, so a flag would still be unset when the
    /// activation path has to decide whether to subscribe. The attribute's own predicate is used so that wildcard
    /// namespaces are matched the same way the runtime matches them.
    /// </remarks>
    protected static bool IsImplicit(StreamId streamId)
    {
        if (ImplicitPredicates.Length == 0)
        {
            return false;
        }

        var streamNamespace = streamId.GetNamespace();
        return streamNamespace is not null
            && Array.Exists(ImplicitPredicates, predicate => predicate.IsMatch(streamNamespace));
    }

    /// <summary>
    /// Ends this grain's subscription so that it will not be resumed on a later activation.
    /// </summary>
    /// <remarks>
    /// Deliberately not called on deactivation. An Orleans subscription is durable and outlives the activation
    /// that created it; unsubscribing when the grain deactivates would discard the consumer's recorded position
    /// and stop the runtime from reactivating it when the next event arrives. Reactivation instead resumes the
    /// existing handle. Call this only when the grain is logically finished with the stream.
    /// </remarks>
    protected async Task UnsubscribeAsync()
    {
        foreach (var (handleId, subscription) in _subscriptions.ToArray())
        {
            // Unsubscribing an implicit handle does not end the subscription: the attribute still applies to the
            // type, so the next matching event reactivates the grain and the runtime supplies a handle again.
            // Ending it here would only discard the recorded position, so it is refused rather than faked.
            if (subscription.IsImplicit)
            {
                Logger.LogWarning(
                    "Ignoring unsubscribe for implicitly subscribed stream {StreamId}; remove [ImplicitStreamSubscription] to stop consuming it.",
                    subscription.Handle.StreamId);
                continue;
            }

            try
            {
                await subscription.Handle.UnsubscribeAsync();
            }
            catch (Exception exception)
            {
                Logger.LogWarning(
                    exception,
                    "Failed to unsubscribe from {Provider}/{StreamId}.",
                    subscription.Handle.ProviderName,
                    subscription.Handle.StreamId);
            }
            finally
            {
                _subscriptions.Remove(handleId);
            }
        }
    }

    private async Task SubscribeAsync(StreamSubscriptionSource source)
    {
        var stream = this.GetStreamProvider(source.ProviderName).GetStream<IStreamedEvent<object>>(source.StreamId);

        // Resume an existing handle when the runtime already has one for this consumer, so that
        // reactivation does not create a duplicate subscription to the same stream.
        var handles = await stream.GetAllSubscriptionHandles();
        var resumeToken = await GetResumeTokenAsync(source);

        var handle = handles.Count > 0
            ? await handles[0].ResumeAsync(HandleNextAsync, HandleErrorAsync, HandleCompletedAsync, resumeToken)
            : await stream.SubscribeAsync(HandleNextAsync, HandleErrorAsync, HandleCompletedAsync, resumeToken);

        _subscriptions[handle.HandleId] = new Subscription(handle, IsImplicit: false);
    }

    private Task HandleNextAsync(IStreamedEvent<object> streamedEvent, StreamSequenceToken? token)
        => OnNextAsync(streamedEvent, token);

    private Task HandleErrorAsync(Exception exception) => OnErrorAsync(exception);

    private Task HandleCompletedAsync() => OnCompletedAsync();
}
