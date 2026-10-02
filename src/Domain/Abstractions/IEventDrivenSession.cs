namespace Continuum.Domain;

/// <summary>A unit of work that applies events to a copy of an event-driven state and tracks uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public interface IEventDrivenSession<TState, TEventBase> : IDisposable
    where TState : IEventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    /// <summary>Gets the current state of the session.</summary>
    TState State { get; }

    /// <summary>Gets the events applied in this session that have not yet been saved.</summary>
    IReadOnlyList<TEventBase> UncommittedEvents { get; }

    /// <summary>Records an event and applies it to the state.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="evt">The event to apply.</param>
    void Apply<TEvent>(TEvent evt) where TEvent : TEventBase;

    /// <summary>Records an event, applies it to the state, and returns the state before and after.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="evt">The event to apply.</param>
    /// <returns>A copy of the state before the event and the state after the event.</returns>
    (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt) where TEvent : TEventBase;
}

/// <summary>A unit of work that applies events to a copy of a state machine backed state and tracks uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> : IDisposable
    where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : IStateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <summary>Gets the current state of the session.</summary>
    TState State { get; }

    /// <summary>Gets the events applied in this session that have not yet been saved.</summary>
    IReadOnlyCollection<TEventBase> UncommittedEvents { get; }

    /// <summary>Records an event and applies it to the state.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="evt">The event to apply.</param>
    void Apply<TEvent>(TEvent evt) where TEvent : TEventBase;

    /// <summary>Records an event, applies it to the state, and returns the state before and after.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="evt">The event to apply.</param>
    /// <returns>A copy of the state before the event and the state after the event.</returns>
    (TState PreviousState, TState CurrentState) Evolve<TEvent>(TEvent evt)
        where TEvent : TEventBase;
}
