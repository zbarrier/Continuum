using System.Collections.ObjectModel;

using Orleans.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>A session over a copy of an <see cref="EventDrivenState{TState, TEventBase}"/> that tracks uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public sealed class EventDrivenSession<TState, TEventBase> : IEventDrivenSession<TState, TEventBase>
    where TState : EventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    private readonly string _key;
    private readonly DeepCopier<TState> _deepCopier;
    private readonly List<TEventBase> _uncommittedEvents = new();
    private readonly ReadOnlyCollection<TEventBase> _uncommittedEventsView;

    /// <summary>Initializes a new instance of the <see cref="EventDrivenSession{TState, TEventBase}"/> class.</summary>
    /// <param name="key">The key of the entity that owns the state.</param>
    /// <param name="deepCopier">The copier used to isolate the session state from the source state.</param>
    /// <param name="state">The state to copy into the session.</param>
    public EventDrivenSession(string key, DeepCopier<TState> deepCopier, TState state)
    {
        _key = key;
        _deepCopier = deepCopier;
        _uncommittedEventsView = _uncommittedEvents.AsReadOnly();
        State = CopyState(state);
    }

    /// <inheritdoc/>
    public TState State { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<TEventBase> UncommittedEvents => _uncommittedEventsView;

    /// <inheritdoc/>
    public void Apply<TEvent>(TEvent evt) where TEvent : TEventBase
    {
        _uncommittedEvents.Add(evt);
        State.When(evt);
    }

    /// <inheritdoc/>
    public (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : TEventBase
    {
        _uncommittedEvents.Add(evt);
        var previousState = CopyState(State);
        State.When(evt);
        return (previousState, State);
    }

    private TState CopyState(TState state) =>
        _deepCopier.Copy(state) ?? throw new InvalidOperationException($"Deep copy of {typeof(TState).Name} returned null.");

    /// <inheritdoc/>
    // Intentionally empty: sessions hold no unmanaged resources; kept so callers use 'using' if cleanup is added later.
    public void Dispose() { }
}
