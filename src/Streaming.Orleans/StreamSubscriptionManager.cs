using Microsoft.Extensions.Logging;

using Orleans.Streams;
using Orleans.Streams.Core;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// Owns the Orleans stream subscriptions held by a grain that consumes events one at a time.
/// </summary>
/// <remarks>
/// <para>
///     Kept separate from any public grain base so that the subscription rules, which are subtle, live in one place
///     and can be shared by every kind of stream consumer, such as projections and process managers, without each
///     inheriting from a common subscriber base.
/// </para>
/// <para>
///     This type is intentionally provider agnostic: it depends only on the Orleans streaming abstractions and never
///     on a specific provider implementation. Orleans orders stream cursors by the <see cref="long"/> in its
///     <c>EventSequenceToken</c>, so a provider whose sequence numbers exceed that range, such as Kinesis, cannot be
///     delivered through it and must be consumed through a catch-up subscription instead.
/// </para>
/// </remarks>
/// <typeparam name="TGrain">The grain type, whose <c>[ImplicitStreamSubscription]</c> attributes are honoured.</typeparam>
internal sealed class StreamSubscriptionManager<TGrain>
    where TGrain : Grain
{
    /// <summary>
    /// Namespace predicates declared by <c>[ImplicitStreamSubscription]</c> on <typeparamref name="TGrain"/>.
    /// </summary>
    /// <remarks>
    /// Static so the reflection runs once per closed generic type rather than on every activation. Because the type
    /// is generic over <typeparamref name="TGrain"/>, each grain type gets its own initializer run, which is exactly
    /// the required granularity.
    /// </remarks>
    private static readonly IStreamNamespacePredicate[] ImplicitPredicates =
        typeof(TGrain)
            .GetCustomAttributes(typeof(ImplicitStreamSubscriptionAttribute), inherit: true)
            .Cast<ImplicitStreamSubscriptionAttribute>()
            .Select(attribute => attribute.Predicate)
            .ToArray();

    private readonly Dictionary<Guid, Subscription> _subscriptions = [];
    private readonly Grain _grain;
    private readonly ILogger _logger;
    private readonly Func<IStreamedEvent<object>, StreamSequenceToken?, Task> _onNextAsync;
    private readonly Func<Exception, Task> _onErrorAsync;
    private readonly Func<Task> _onCompletedAsync;

    /// <summary>
    /// A subscription handle together with how it was established.
    /// </summary>
    /// <remarks>
    /// The origin is recorded where it is known rather than inferred later: the runtime only calls
    /// <see cref="AttachImplicitAsync"/> for subscriptions it created, and everything produced by
    /// <see cref="SubscribeAsync"/> is by definition explicit.
    /// </remarks>
    private readonly record struct Subscription(
        StreamSubscriptionHandle<IStreamedEvent<object>> Handle,
        bool IsImplicit);

    public StreamSubscriptionManager(
        Grain grain,
        ILogger logger,
        Func<IStreamedEvent<object>, StreamSequenceToken?, Task> onNextAsync,
        Func<Exception, Task> onErrorAsync,
        Func<Task> onCompletedAsync)
    {
        _grain = grain;
        _logger = logger;
        _onNextAsync = onNextAsync;
        _onErrorAsync = onErrorAsync;
        _onCompletedAsync = onCompletedAsync;
    }

    /// <summary>
    /// Subscribes to every explicit source, skipping streams the runtime covers through an implicit subscription.
    /// </summary>
    /// <remarks>
    /// An implicit subscription activates the grain because an event arrived, and the runtime then calls
    /// <c>OnSubscribed</c> with a handle already positioned at that event. Subscribing here as well would attach a
    /// second handle at the current position, so streams the runtime covers are skipped. The check is per stream
    /// rather than per grain so that a grain may combine implicit and explicit subscriptions.
    /// </remarks>
    public async Task SubscribeAllAsync(
        IEnumerable<StreamSubscriptionSource> sources,
        Func<StreamSubscriptionSource, Task<StreamSequenceToken?>> getResumeTokenAsync)
    {
        foreach (var source in sources)
        {
            await SubscribeAsync(source, getResumeTokenAsync);
        }
    }

    /// <summary>
    /// Attaches to the subscription the runtime created for an implicit subscriber.
    /// </summary>
    /// <remarks>
    /// The handle carries the sequence token of the event that triggered the activation, so attaching to it
    /// delivers that event. Subscribing independently would start from the current position and lose it.
    /// </remarks>
    public async Task AttachImplicitAsync(IStreamSubscriptionHandleFactory handleFactory)
    {
        var handle = handleFactory.Create<IStreamedEvent<object>>();
        var resumed = await handle.ResumeAsync(_onNextAsync, _onErrorAsync, _onCompletedAsync);

        // Keyed by handle id because the runtime calls this once per implicit subscription, and two handles may
        // refer to the same stream. Keying by stream would let the second call discard the first handle, which
        // would leak a live subscription that could no longer be unsubscribed.
        _subscriptions[resumed.HandleId] = new Subscription(resumed, IsImplicit: true);
    }

    /// <summary>
    /// Whether the runtime supplies subscriptions for the given stream because of an implicit subscription.
    /// </summary>
    /// <remarks>
    /// Determined from the attributes rather than from a flag set when attaching, because the runtime calls
    /// <c>OnActivateAsync</c> before <c>OnSubscribed</c>, so a flag would still be unset when the activation path has
    /// to decide whether to subscribe. The attribute's own predicate is used so that wildcard namespaces are matched
    /// the same way the runtime matches them.
    /// </remarks>
    public static bool IsImplicit(StreamId streamId)
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
    /// Ends the explicit subscriptions so that they will not be resumed on a later activation.
    /// </summary>
    /// <remarks>
    /// Unsubscribing an implicit handle does not end the subscription: the attribute still applies to the type, so
    /// the next matching event reactivates the grain and the runtime supplies a handle again. Ending it here would
    /// only discard the recorded position, so it is refused rather than faked.
    /// </remarks>
    public Task UnsubscribeAsync() => UnsubscribeWhereAsync(static _ => true);

    /// <summary>
    /// Ends the explicit subscription to a single stream so that it will not be resumed on a later activation.
    /// </summary>
    /// <remarks>
    /// Does nothing if the stream is not currently subscribed. Implicit subscriptions are refused for the same
    /// reason as in <see cref="UnsubscribeAsync()"/>.
    /// </remarks>
    public Task UnsubscribeAsync(StreamSubscriptionSource source)
        => UnsubscribeWhereAsync(subscription => Matches(subscription, source));

    /// <summary>
    /// Subscribes to a single stream, or resumes the existing subscription to it.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Does nothing if the stream is already subscribed by this activation. A stream covered by an implicit
    ///     subscription is skipped, because the runtime supplies its handle.
    /// </para>
    /// <para>
    ///     The subscription is durable, but it is only resumed on a later activation if the grain includes the stream
    ///     in the sources it passes to <see cref="SubscribeAllAsync"/>. A grain that subscribes at runtime must
    ///     therefore record the stream in its own state and return it from its subscription list.
    /// </para>
    /// </remarks>
    public Task SubscribeAsync(
        StreamSubscriptionSource source,
        Func<StreamSubscriptionSource, Task<StreamSequenceToken?>> getResumeTokenAsync)
    {
        if (IsImplicit(source.StreamId))
        {
            _logger.LogDebug(
                "Skipping explicit subscribe for implicitly subscribed stream {StreamId}.",
                source.StreamId);
            return Task.CompletedTask;
        }

        if (_subscriptions.Values.Any(subscription => Matches(subscription, source)))
        {
            return Task.CompletedTask;
        }

        return SubscribeCoreAsync(source, getResumeTokenAsync);
    }

    private async Task UnsubscribeWhereAsync(Func<Subscription, bool> predicate)
    {
        foreach (var (handleId, subscription) in _subscriptions.ToArray())
        {
            if (!predicate(subscription))
            {
                continue;
            }

            if (subscription.IsImplicit)
            {
                _logger.LogWarning(
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
                _logger.LogWarning(
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

    private static bool Matches(Subscription subscription, StreamSubscriptionSource source)
        => subscription.Handle.StreamId.Equals(source.StreamId)
            && string.Equals(subscription.Handle.ProviderName, source.ProviderName, StringComparison.Ordinal);

    /// <summary>
    /// Describes the subscriptions currently held, for diagnostics.
    /// </summary>
    public string DescribeSubscriptions()
        => _subscriptions.Count == 0
            ? "(none)"
            : string.Join(
                ", ",
                _subscriptions.Values.Select(subscription => string.Concat(
                    subscription.Handle.ProviderName,
                    "/",
                    subscription.Handle.StreamId.ToString(),
                    subscription.IsImplicit ? " (implicit)" : " (explicit)")));

    private async Task SubscribeCoreAsync(
        StreamSubscriptionSource source,
        Func<StreamSubscriptionSource, Task<StreamSequenceToken?>> getResumeTokenAsync)
    {
        var stream = _grain.GetStreamProvider(source.ProviderName).GetStream<IStreamedEvent<object>>(source.StreamId);

        // Resume an existing handle when the runtime already has one for this consumer, so that
        // reactivation does not create a duplicate subscription to the same stream.
        var handles = await stream.GetAllSubscriptionHandles();
        var resumeToken = await getResumeTokenAsync(source);

        var handle = handles.Count > 0
            ? await handles[0].ResumeAsync(_onNextAsync, _onErrorAsync, _onCompletedAsync, resumeToken)
            : resumeToken is null
                ? await stream.SubscribeAsync(_onNextAsync, _onErrorAsync, _onCompletedAsync)
                : await stream.SubscribeAsync(_onNextAsync, _onErrorAsync, _onCompletedAsync, resumeToken);

        _subscriptions[handle.HandleId] = new Subscription(handle, IsImplicit: false);
    }
}
