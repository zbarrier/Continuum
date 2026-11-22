using System.Numerics;

namespace Continuum.Streaming.Orleans;

/// <summary>
/// State maintained by a <see cref="ProjectionGrain{TGrain, TState, TContext}"/>.
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
}

/// <summary>
/// Projection state that applies events with the dependencies in <typeparamref name="TContext"/>.
/// </summary>
/// <typeparam name="TContext">The dependencies supplied by the grain, or <see cref="NoProjectionContext"/>.</typeparam>
public interface IProjectionState<TContext> : IProjectionState
    where TContext : class
{
    /// <summary>
    /// Applies a single event if it has not already been applied.
    /// </summary>
    /// <returns>True if the state changed and must be written.</returns>
    Task<bool> WhenAsync(IStreamedEvent<object> streamedEvent, TContext context);

    /// <summary>
    /// Applies each event in order, skipping any that have already been applied.
    /// </summary>
    /// <returns>True if any event changed the state and it must be written.</returns>
    Task<bool> WhenAsync(IReadOnlyList<IStreamedEvent<object>> streamedEvents, TContext context);
}

/// <summary>
/// Base class for projection state whose events are applied with dependencies supplied by the grain.
/// </summary>
/// <remarks>
/// <para>
///     Both delivery paths share one de-duplication watermark, so an event seen through one path is not
///     applied again when it arrives through the other. The watermark is compared on
///     <see cref="StreamedEventCursor.SequenceNumber"/> rather than on a provider-specific token, so the same
///     state works across providers whose positions are a <see cref="BigInteger"/> as well as those that
///     use a 64-bit position.
/// </para>
/// <para>
///     The context is passed with every call rather than held by the state, because the state instance is replaced
///     whenever it is read or cleared, and the state stays plain serializable data. Use
///     <see cref="ProjectionState{TState}"/> when no dependencies are needed.
/// </para>
/// <para>
///     A lookup made while applying an event returns the current data, not the data as it was when the event
///     occurred, so a replay can project a different result. For a repeatable projection, have the event carry the
///     ID and version of anything it needs to look up.
/// </para>
/// </remarks>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TContext">The dependencies supplied by the grain.</typeparam>
[GenerateSerializer]
public abstract class ProjectionState<TState, TContext> : IProjectionState<TContext>
    where TState : ProjectionState<TState, TContext>
    where TContext : class
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
    public virtual async Task<bool> WhenAsync(IStreamedEvent<object> streamedEvent, TContext context)
    {
        ArgumentNullException.ThrowIfNull(streamedEvent);
        ArgumentNullException.ThrowIfNull(context);

        if (Watermarks.IsApplied(streamedEvent.Cursor))
        {
            // Already applied. A redelivery after a failed write, or a rewind, must not double-apply.
            return false;
        }

        var hasChanges = await Handle(streamedEvent, context);
        Watermarks.Advance(streamedEvent.Cursor);
        return hasChanges;
    }

    /// <inheritdoc/>
    public virtual async Task<bool> WhenAsync(IReadOnlyList<IStreamedEvent<object>> streamedEvents, TContext context)
    {
        ArgumentNullException.ThrowIfNull(streamedEvents);

        var hasChanges = false;
        foreach (var streamedEvent in streamedEvents)
        {
            hasChanges |= await WhenAsync(streamedEvent, context);
        }

        return hasChanges;
    }

    /// <summary>
    /// Handles the given event by updating the state.
    /// </summary>
    /// <returns>True if the event was applied and false if the event was ignored.</returns>
    protected abstract Task<bool> Handle(IStreamedEvent<object> streamedEvent, TContext context);
}

/// <summary>
/// Base class for projection state that needs no dependencies to apply its events.
/// </summary>
/// <remarks>
/// Use <see cref="ProjectionState{TState, TContext}"/> when applying an event needs dependencies such as other
/// grains or services.
/// </remarks>
/// <typeparam name="TState">The concrete state type.</typeparam>
[GenerateSerializer]
public abstract class ProjectionState<TState> : ProjectionState<TState, NoProjectionContext>
    where TState : ProjectionState<TState>
{
    /// <inheritdoc/>
    protected sealed override Task<bool> Handle(IStreamedEvent<object> streamedEvent, NoProjectionContext context)
        => Handle(streamedEvent);

    /// <summary>
    /// Handles the given event by updating the state.
    /// </summary>
    /// <returns>True if the event was applied and false if the event was ignored.</returns>
    protected abstract Task<bool> Handle(IStreamedEvent<object> streamedEvent);
}

/// <summary>
/// Applies events to projection state that needs no dependencies.
/// </summary>
public static class ProjectionStateExtensions
{
    /// <summary>
    /// Applies a single event if it has not already been applied.
    /// </summary>
    /// <returns>True if the state changed and must be written.</returns>
    public static Task<bool> WhenAsync(
        this IProjectionState<NoProjectionContext> state,
        IStreamedEvent<object> streamedEvent)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.WhenAsync(streamedEvent, NoProjectionContext.Instance);
    }

    /// <summary>
    /// Applies each event in order, skipping any that have already been applied.
    /// </summary>
    /// <returns>True if any event changed the state and it must be written.</returns>
    public static Task<bool> WhenAsync(
        this IProjectionState<NoProjectionContext> state,
        IReadOnlyList<IStreamedEvent<object>> streamedEvents)
    {
        ArgumentNullException.ThrowIfNull(state);
        return state.WhenAsync(streamedEvents, NoProjectionContext.Instance);
    }
}
