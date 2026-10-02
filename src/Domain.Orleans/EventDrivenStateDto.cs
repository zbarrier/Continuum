namespace Continuum.Domain.Orleans;

/// <summary>Base class for a DTO that is built by dispatching events to registered handlers.</summary>
[Alias("Continuum.EventDrivenStateDto.V1"), GenerateSerializer]
public abstract class EventDrivenStateDto
{
    [NonSerialized] private readonly Dictionary<Type, Action<object>> _handlers = new();

    /// <summary>Registers the handler invoked when an event of type <typeparamref name="TEvent"/> is applied.</summary>
    /// <typeparam name="TEvent">The event type to handle.</typeparam>
    /// <param name="handler">The handler to invoke.</param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Action<TEvent> handler)
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

    /// <summary>Applies an event by invoking its registered handler, if any.</summary>
    /// <param name="evt">The event to apply.</param>
    /// <remarks>Only the handler registered for the exact runtime type of the event is invoked; handlers for base types are not matched. Events without a handler are ignored.</remarks>
    public virtual void When(object evt)
    {
        var eventType = evt.GetType();
        if (_handlers.TryGetValue(eventType, out var handler))
        {
            handler(evt);
        }
    }
}
