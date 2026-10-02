using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain;

/// <summary>Extension methods that run operations inside an event-driven session scope as part of a <see cref="Result"/> pipeline.</summary>
public static partial class ResultSessionScopeExtensions
{

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Action<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                f(session);
                return await sessionScope.SaveChanges(session);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, Result> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = f(session);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand, T>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, T, Result> f,
            T value)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = f(session, value);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, Task<Result>> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = await f(session);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand, T>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, T, Task<Result>> f,
            T value)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()

        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = await f(session, value);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, ValueTask<Result>> f)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = await f(session);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, TStatus, TCommand, T>(
            IEventDrivenStateMachineSessionScope<TState, TEventBase, TStatus, TCommand> sessionScope,
            Func<IEventDrivenStateMachineSession<TState, TEventBase, TStatus, TCommand>, T, ValueTask<Result>> f,
            T value)
        where TState : IEventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : IStateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        using (var session = sessionScope.CreateSession())
        {
            try
            {
                var result = await f(session, value);
                if (result.IsSuccess)
                {
                    return await sessionScope.SaveChanges(session);
                }
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            catch (Exception ex)
            {
                return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
            }
        }
    }
}
