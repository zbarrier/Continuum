namespace Continuum.Streaming.Orleans;

/// <summary>
/// Projection state that applies events, with dependencies supplied by the grain, through handlers registered in its
/// constructor.
/// </summary>
/// <remarks>
/// <para>
///     An alternative to overriding <see cref="ProjectionState{TState, TContext}.Handle"/> directly. A state either
///     registers handlers or overrides <c>Handle</c>, never both, so <c>Handle</c> is sealed here.
/// </para>
/// <para>
///     Only the handler registered for the exact runtime type of the event runs; handlers for base types are not
///     matched. Events without a handler are ignored and do not cause a write. Override <c>Handle</c> with a
///     <see langword="switch"/> instead when events must be matched by a base type.
/// </para>
/// <para>
///     Register handlers in the parameterless constructor, which runs every time the state is created, including
///     when it is read back from storage. Use <see cref="HandlerProjectionState{TState}"/> when no dependencies are
///     needed.
/// </para>
/// </remarks>
/// <typeparam name="TState">The concrete state type.</typeparam>
/// <typeparam name="TContext">The dependencies supplied by the grain.</typeparam>
[GenerateSerializer]
public abstract class HandlerProjectionState<TState, TContext> : ProjectionState<TState, TContext>
    where TState : HandlerProjectionState<TState, TContext>
    where TContext : class
{
    [NonSerialized]
    private readonly Dictionary<Type, Func<IStreamedEvent<object>, TContext, Task<bool>>> _handlers = new();

    /// <summary>
    /// Registers the handler invoked when an event of type <typeparamref name="TEvent"/> is applied.
    /// </summary>
    /// <param name="handler">
    /// Receives the event, its envelope and the context, and returns <see langword="true"/> if it changed the state
    /// and the state must be written.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Func<TEvent, IStreamedEvent<object>, TContext, bool> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Register<TEvent>((streamedEvent, context)
            => Task.FromResult(handler((TEvent)streamedEvent.Event, streamedEvent, context)));
    }

    /// <summary>
    /// Registers the asynchronous handler invoked when an event of type <typeparamref name="TEvent"/> is applied.
    /// </summary>
    /// <param name="handler">
    /// Receives the event, its envelope and the context, and returns <see langword="true"/> if it changed the state
    /// and the state must be written.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Func<TEvent, IStreamedEvent<object>, TContext, Task<bool>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Register<TEvent>((streamedEvent, context) => handler((TEvent)streamedEvent.Event, streamedEvent, context));
    }

    /// <summary>
    /// Invokes the handler registered for the event's exact runtime type.
    /// </summary>
    /// <returns>The handler's result, or <see langword="false"/> if no handler is registered.</returns>
    protected sealed override Task<bool> Handle(IStreamedEvent<object> streamedEvent, TContext context)
        => _handlers.TryGetValue(streamedEvent.Event.GetType(), out var handler)
            ? handler(streamedEvent, context)
            : Task.FromResult(false);

    private void Register<TEvent>(Func<IStreamedEvent<object>, TContext, Task<bool>> handler)
    {
        if (!_handlers.TryAdd(typeof(TEvent), handler))
        {
            throw new InvalidOperationException($"A handler for {typeof(TEvent).Name} is already registered.");
        }
    }
}

/// <summary>
/// Projection state that needs no dependencies and applies events through handlers registered in its constructor.
/// </summary>
/// <remarks>
/// Matching follows <see cref="HandlerProjectionState{TState, TContext}"/>: only the exact runtime type is matched,
/// and events without a handler are ignored.
/// </remarks>
/// <typeparam name="TState">The concrete state type.</typeparam>
[GenerateSerializer]
public abstract class HandlerProjectionState<TState> : HandlerProjectionState<TState, NoProjectionContext>
    where TState : HandlerProjectionState<TState>
{
    /// <summary>
    /// Registers the handler invoked when an event of type <typeparamref name="TEvent"/> is applied.
    /// </summary>
    /// <param name="handler">
    /// Receives the event and its envelope, and returns <see langword="true"/> if it changed the state and the state
    /// must be written.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Func<TEvent, IStreamedEvent<object>, bool> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        On<TEvent>((evt, streamedEvent, _) => handler(evt, streamedEvent));
    }

    /// <summary>
    /// Registers the asynchronous handler invoked when an event of type <typeparamref name="TEvent"/> is applied.
    /// </summary>
    /// <param name="handler">
    /// Receives the event and its envelope, and returns <see langword="true"/> if it changed the state and the state
    /// must be written.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">A handler is already registered for <typeparamref name="TEvent"/>.</exception>
    protected void On<TEvent>(Func<TEvent, IStreamedEvent<object>, Task<bool>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        On<TEvent>((evt, streamedEvent, _) => handler(evt, streamedEvent));
    }
}
