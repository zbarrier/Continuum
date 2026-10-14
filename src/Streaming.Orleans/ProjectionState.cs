using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// State maintained by a <see cref="ProjectionGrain{TGrain, TState}"/>.
/// </summary>
public interface IProjectionState
{
    /// <summary>
    /// The highest position applied so far, per topic and partition, used to skip events that have already been
    /// applied.
    /// </summary>
    ProjectionWatermarks Watermarks { get; }

    /// <summary>
    /// The streams subscribed to at runtime, which are resumed when the grain reactivates.
    /// </summary>
    /// <remarks>
    /// Streams declared statically through the grain's subscription list are not recorded here.
    /// </remarks>
    IReadOnlyCollection<StreamSubscriptionSource> Subscriptions { get; }

    /// <summary>
    /// Records a stream subscribed to at runtime.
    /// </summary>
    /// <returns>True if the stream was not already recorded and the state must be written.</returns>
    bool AddSubscription(StreamSubscriptionSource source);

    /// <summary>
    /// Removes a stream recorded by <see cref="AddSubscription"/>.
    /// </summary>
    /// <remarks>
    /// The stream's sequence watermark is kept, so subscribing to it again later does not reapply events that were
    /// already projected.
    /// </remarks>
    /// <returns>True if the stream was recorded and the state must be written.</returns>
    bool RemoveSubscription(StreamSubscriptionSource source);

    /// <summary>
    /// Removes every stream recorded by <see cref="AddSubscription"/>.
    /// </summary>
    /// <returns>True if any stream was recorded and the state must be written.</returns>
    bool ClearSubscriptions();

    /// <summary>
    /// Applies a single event if it has not already been applied.
    /// </summary>
    /// <returns>True if the state changed and must be written.</returns>
    ValueTask<bool> WhenAsync(IStreamedEvent<object> streamedEvent);

    /// <summary>
    /// Applies each event in order, skipping any that have already been applied.
    /// </summary>
    /// <returns>True if any event changed the state and it must be written.</returns>
    ValueTask<bool> WhenAsync(IReadOnlyList<IStreamedEvent<object>> streamedEvents);
}

/// <summary>
/// Base class for projection state, applied from either single-event stream delivery or a catch-up batch.
/// </summary>
/// <remarks>
/// Both delivery paths share one de-duplication watermark, so an event seen through one path is not
/// applied again when it arrives through the other. The watermark is compared on
/// <see cref="StreamedEventCursor.SequenceNumber"/> rather than on a provider-specific token, so the same
/// state works across providers whose positions are a <see cref="BigInteger"/> as well as those that
/// use a 64-bit position.
/// </remarks>
[GenerateSerializer]
public abstract class ProjectionState<TState> : IProjectionState
    where TState : ProjectionState<TState>
{
    /// <inheritdoc/>
    [Id(0)]
    public ProjectionWatermarks Watermarks { get; } = new();

    [Id(1)]
    private readonly List<StreamSubscriptionSource> _subscriptions = [];

    /// <inheritdoc/>
    public IReadOnlyCollection<StreamSubscriptionSource> Subscriptions => _subscriptions;

    /// <inheritdoc/>
    public bool AddSubscription(StreamSubscriptionSource source)
    {
        if (_subscriptions.Contains(source))
        {
            return false;
        }

        _subscriptions.Add(source);
        return true;
    }

    /// <inheritdoc/>
    public bool RemoveSubscription(StreamSubscriptionSource source) => _subscriptions.Remove(source);

    /// <inheritdoc/>
    public bool ClearSubscriptions()
    {
        if (_subscriptions.Count == 0)
        {
            return false;
        }

        _subscriptions.Clear();
        return true;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<bool> WhenAsync(IStreamedEvent<object> streamedEvent)
    {
        ArgumentNullException.ThrowIfNull(streamedEvent);

        if (Watermarks.IsApplied(streamedEvent.Cursor))
        {
            // Already applied. A redelivery after a failed write, or a rewind, must not double-apply.
            return false;
        }

        var hasChanges = await Handle(streamedEvent);
        Watermarks.Advance(streamedEvent.Cursor);
        return hasChanges;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<bool> WhenAsync(IReadOnlyList<IStreamedEvent<object>> streamedEvents)
    {
        ArgumentNullException.ThrowIfNull(streamedEvents);

        var hasChanges = false;
        foreach (var streamedEvent in streamedEvents)
        {
            hasChanges |= await WhenAsync(streamedEvent);
        }

        return hasChanges;
    }

    /// <summary>
    /// Handles the given event by updating the state.
    /// </summary>
    /// <returns>True if the event was applied and false if the event was ignored.</returns>
    protected abstract ValueTask<bool> Handle(IStreamedEvent<object> streamedEvent);
}
