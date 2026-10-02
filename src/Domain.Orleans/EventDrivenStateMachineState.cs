using Stateless;

namespace Continuum.Domain.Orleans;

/// <summary>Base class for an event-driven state backed by a <see cref="StateMachine{TState, TTrigger}"/>; each applied event fires its command on the state machine.</summary>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
[Alias("Continuum.EventDrivenStateMachineState.V1`4"), GenerateSerializer]
public abstract class EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand> : 
    IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand> 
    where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : StateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    [NonSerialized] private readonly Dictionary<Type, Action<IStateMachineEvent<TCommand>>> _handlers = new();

    [Id(0)] private readonly StateMachine<TStatus, TCommand> _stateMachine;

    /// <summary>Initializes a new instance, registering event handlers and creating and configuring the state machine.</summary>
    /// <param name="createStateMachine">Creates the state machine in its initial status.</param>
    protected EventDrivenStateMachineState(Func<StateMachine<TStatus, TCommand>> createStateMachine)
    {
        ConfigureEventHandlers();
        if (_stateMachine is null)
        {
            _stateMachine = createStateMachine();
            ConfigureStateMachine();
        }
    }

    /// <summary>Gets the state machine that tracks the status of the state.</summary>
    public StateMachine<TStatus, TCommand> StateMachine => _stateMachine;

    /// <summary>Registers the handler invoked when an event of type <typeparamref name="TEvent"/> is applied.</summary>
    /// <typeparam name="TEvent">The event type to handle.</typeparam>
    /// <param name="handler">The handler to invoke.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Action<TEvent> handler) where TEvent : IStateMachineEvent<TCommand>
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        if (!_handlers.TryAdd(typeof(TEvent), (evt) => handler((TEvent)evt)))
        {
            throw new InvalidOperationException("Duplicate event handler.");
        }
    }

    /// <inheritdoc/>
    /// <remarks>Only the handler registered for the exact runtime type of the event is invoked; handlers for base types are not matched. Events without a handler are ignored.</remarks>
    public virtual void When(IStateMachineEvent<TCommand> evt)
    {
        var eventType = evt.GetType();
        if (_handlers.TryGetValue(eventType, out var handler))
        {
            handler(evt);
        }
        _stateMachine.Fire(evt.Command);
    }

    /// <summary>Registers event handlers by calling <see cref="On{TEvent}(Action{TEvent})"/>.</summary>
    /// <remarks>Called from the base constructor before derived constructors run; handlers must not depend on fields set in a derived constructor.</remarks>
    protected abstract void ConfigureEventHandlers();

    /// <summary>Configures the permitted transitions of <see cref="StateMachine"/>.</summary>
    /// <remarks>Deep copies restore only the current status, not transitions, so this is called again on every copied state.</remarks>
    protected internal abstract void ConfigureStateMachine();
}
