namespace Continuum.Domain;

/// <summary>A state that evolves by applying events.</summary>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public interface IEventDrivenState<TState, TEventBase>
    where TState : IEventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    /// <summary>Applies an event to the state.</summary>
    /// <param name="event">The event to apply.</param>
    void When(TEventBase @event);
}

/// <summary>An event-driven state backed by a state machine.</summary>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>
    where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : IStateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <summary>Applies an event to the state.</summary>
    /// <param name="event">The event to apply.</param>
    void When(IStateMachineEvent<TCommand> @event);
}
