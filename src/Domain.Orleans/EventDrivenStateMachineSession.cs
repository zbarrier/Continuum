using System.Collections.ObjectModel;

using Orleans.Serialization;

namespace Continuum.Domain.Orleans;

/// <summary>A session over a copy of an <see cref="EventDrivenStateMachineState{TState, TEventBase, TStatus, TCommand}"/> that tracks uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public sealed class EventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> :
    IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>
    where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : StateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    private readonly DeepCopier<TState> _deepCopier;
    private readonly List<TEventBase> _uncommittedEvents = new();
    private readonly ReadOnlyCollection<TEventBase> _uncommittedEventsView;

    /// <summary>Initializes a new instance of the <see cref="EventDrivenStateMachineSession{TState, TEventBase, TStatus, TCommand}"/> class.</summary>
    /// <param name="deepCopier">The copier used to isolate the session state from the source state.</param>
    /// <param name="state">The state to copy into the session.</param>
    public EventDrivenStateMachineSession(DeepCopier<TState> deepCopier, TState state)
    {
        _deepCopier = deepCopier;
        _uncommittedEventsView = _uncommittedEvents.AsReadOnly();

        State = CopyState(state);
        // Deep copies restore only the current status; transitions must be configured again.
        State.ConfigureStateMachine();
    }

    /// <inheritdoc/>
    public TState State { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyCollection<TEventBase> UncommittedEvents => _uncommittedEventsView;

    /// <inheritdoc/>
    public void Apply<TEvent>(TEvent evt) where TEvent : TEventBase
    {
        _uncommittedEvents.Add(evt);
        State.When(evt);
    }

    /// <inheritdoc/>
    public (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt)
        where TEvent : TEventBase
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
