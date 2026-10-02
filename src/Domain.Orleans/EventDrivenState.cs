namespace Continuum.Domain.Orleans;

/// <summary>Base class for a state that evolves by dispatching events to registered handlers.</summary>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
[Alias("Continuum.EventDrivenState.V1`2"), GenerateSerializer]
public abstract class EventDrivenState<TState, TEventBase> : IEventDrivenState<TState, TEventBase> 
    where TState : EventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    [NonSerialized] private readonly Dictionary<Type, Action<TEventBase>> _handlers = new();

    /// <summary>Registers the handler invoked when an event of type <typeparamref name="TEvent"/> is applied.</summary>
    /// <typeparam name="TEvent">The event type to handle.</typeparam>
    /// <param name="handler">The handler to invoke.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Action<TEvent> handler) where TEvent : TEventBase
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
    public virtual void When(TEventBase evt)
    {
        var eventType = evt.GetType();
        if (_handlers.TryGetValue(eventType, out var handler))
        {
            handler(evt);
        }
    }
}
