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
    /// <param name="self">The source result.</param>
    /// <param name="sessionScope">The session scope used to create and save the session.</param>
    /// <param name="f">The operation to run against the session.</param>
    /// <returns>The result of saving the session, or the failure of the source result or operation.</returns>
    public static async Task<Result<ISaveChangesResponse>> BindWithSessionScope<TState, TEventBase>(this ValueTask<Result> self,
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, ValueTask<Result>> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
    {
        try
        {
            var result = await self;
            if (result.IsFailure)
            {
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            return await WithSessionScope(sessionScope, f);
        }
        catch (Exception ex)
        {
            return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
        }
    }

    /// <summary>
    /// When <paramref name="self"/> succeeds, creates a session from <paramref name="sessionScope"/>, runs <paramref name="f"/> against it,
    /// and saves the session's uncommitted events; otherwise propagates the failure.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
    /// <typeparam name="T">The type of the value carried by the source result.</typeparam>
    /// <param name="self">The source result.</param>
    /// <param name="sessionScope">The session scope used to create and save the session.</param>
    /// <param name="f">The operation to run against the session.</param>
    /// <returns>The result of saving the session, or the failure of the source result or operation.</returns>
    public static async Task<Result<ISaveChangesResponse>> BindWithSessionScope<TState, TEventBase, T>(this ValueTask<Result<T>> self,
            IEventDrivenSessionScope<TState, TEventBase> sessionScope,
            Func<IEventDrivenSession<TState, TEventBase>, T, ValueTask<Result>> f)
        where TState : IEventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
    {
        try
        {
            var result = await self;
            if (result.IsFailure)
            {
                return Result.Failure<ISaveChangesResponse>(result.Error);
            }
            return await WithSessionScope(sessionScope, f, result.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<ISaveChangesResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(sessionScope.Logger, ex));
        }
    }
}
