using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain;

/// <summary>Extension methods that run operations inside an event-driven session scope as part of a <see cref="Result"/> pipeline.</summary>
public static partial class ResultSessionScopeExtensions
{

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Action<IEventDrivenSession<TState, TEventBase>> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, Result> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, T>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, T, Result> f,
            T value)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, Task<Result>> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, T>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, T, Task<Result>> f,
            T value)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, ValueTask<Result>> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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

    private static async Task<Result<ISaveChangesResponse>> WithSessionScope<TState, TEventBase, T>(
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, T, ValueTask<Result>> f,
            T value)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
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
