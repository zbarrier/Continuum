using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain;

/// <summary>Extension methods that run operations inside an event-driven session scope as part of a <see cref="Result"/> pipeline.</summary>
public static partial class ResultSessionScopeExtensions
{

    /// <summary>
    /// When <paramref name="self"/> succeeds, creates a session from <paramref name="sessionScope"/>, runs <paramref name="f"/> against it,
    /// and saves the session's uncommitted events; otherwise propagates the failure.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
    /// <typeparam name="TStatus">The state machine status type.</typeparam>
    /// <typeparam name="TCommand">The state machine trigger type.</typeparam>
    /// <param name="self">The source result.</param>
    /// <param name="sessionScope">The session scope used to create and save the session.</param>
    /// <param name="f">The operation to run against the session.</param>
    /// <returns>The result of saving the session, or the failure of the source result or operation.</returns>
    public static Task<Result<ISaveChangesResponse>> BindWithSessionScope<TState, TEventBase, TStatus, TCommand>(this Result self,
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, ValueTask<Result>> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        if (self.IsFailure)
        {
            return Task.FromResult(Result.Failure<ISaveChangesResponse>(self.Error));
        }
        return WithSessionScope(sessionScope, f);
    }

    /// <summary>
    /// When <paramref name="self"/> succeeds, creates a session from <paramref name="sessionScope"/>, runs <paramref name="f"/> against it,
    /// and saves the session's uncommitted events; otherwise propagates the failure.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
    /// <typeparam name="TStatus">The state machine status type.</typeparam>
    /// <typeparam name="TCommand">The state machine trigger type.</typeparam>
    /// <typeparam name="T">The type of the value carried by the source result.</typeparam>
    /// <param name="self">The source result.</param>
    /// <param name="sessionScope">The session scope used to create and save the session.</param>
    /// <param name="f">The operation to run against the session.</param>
    /// <returns>The result of saving the session, or the failure of the source result or operation.</returns>
    public static Task<Result<ISaveChangesResponse>> BindWithSessionScope<TState, TEventBase, TStatus, TCommand, T>(this Result<T> self,
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, T, ValueTask<Result>> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        if (self.IsFailure)
        {
            return Task.FromResult(Result.Failure<ISaveChangesResponse>(self.Error));
        }
        return WithSessionScope(sessionScope, f, self.Value);
    }
}
