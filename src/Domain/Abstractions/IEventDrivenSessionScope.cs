using Continuum.CSharpFunctionalExtensions;

using Microsoft.Extensions.Logging;

namespace Continuum.Domain;

/// <summary>Creates sessions for an event-driven entity and saves their uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
public interface IEventDrivenSessionScope<TState, TEventBase>
    where TState : IEventDrivenState<TState, TEventBase>, new()
    where TEventBase : class
{
    /// <summary>Gets the logger used to record exceptions thrown while running or saving a session.</summary>
    ILogger Logger { get; }

    /// <summary>Creates a new session over a copy of the current state.</summary>
    /// <returns>The new session.</returns>
    IEventDrivenSession<TState, TEventBase> CreateSession();

    /// <summary>Saves the uncommitted events of a session.</summary>
    /// <param name="session">The session to save.</param>
    /// <returns>The save response, or a failure.</returns>
    Task<Result<ISaveChangesResponse>> SaveChanges(IEventDrivenSession<TState, TEventBase> session);
}

/// <summary>Creates sessions for a state machine backed entity and saves their uncommitted events.</summary>
/// <typeparam name="TState">The state type.</typeparam>
/// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
/// <typeparam name="TStatus">The state machine status type.</typeparam>
/// <typeparam name="TCommand">The state machine trigger type.</typeparam>
public interface IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand>
    where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
    where TEventBase : IStateMachineEvent<TCommand>
    where TStatus : notnull
    where TCommand : notnull
{
    /// <summary>Gets the logger used to record exceptions thrown while running or saving a session.</summary>
    ILogger Logger { get; }

    /// <summary>Creates a new session over a copy of the current state.</summary>
    /// <returns>The new session.</returns>
    IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> CreateSession();

    /// <summary>Saves the uncommitted events of a session.</summary>
    /// <param name="session">The session to save.</param>
    /// <returns>The save response, or a failure.</returns>
    Task<Result<ISaveChangesResponse>> SaveChanges(IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand> session);
}
