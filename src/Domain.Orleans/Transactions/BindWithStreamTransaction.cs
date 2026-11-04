using Continuum.CSharpFunctionalExtensions;

namespace Continuum.Domain.Orleans;

/// <summary>Extension methods that run operations inside a stream transaction as part of a <see cref="Result"/> pipeline.</summary>
public static class ResultStreamTransactionExtensions
{
    /// <summary>
    /// When <paramref name="self"/> succeeds, begins a stream transaction on <paramref name="grain"/>, runs
    /// <paramref name="f"/> against it and saves it; otherwise propagates the failure. Events applied through the
    /// transaction go to the grain's own stream. When participants are prepared, their events are saved atomically with
    /// the grain's own events. When <paramref name="f"/> fails or throws, or the save fails, every prepared participant
    /// is rolled back.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
    /// <param name="self">The source result.</param>
    /// <param name="grain">The grain that owns the transaction.</param>
    /// <param name="f">The operation to run against the transaction.</param>
    /// <param name="timeout">How long the transaction may take when participants are prepared; defaults to 30 seconds.</param>
    /// <returns>The result of saving the transaction, or the failure of the source result, the operation or the save.</returns>
    public static Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Result self,
            EventDrivenGrain<TState, TEventBase> grain,
            Action<ICoordinatingStreamTransaction<TState, TEventBase>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
        => self.BindWithStreamTransaction(grain, t => { f(t); return Task.FromResult(Result.Success()); }, timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase}(Result, EventDrivenGrain{TState, TEventBase}, Action{ICoordinatingStreamTransaction{TState, TEventBase}}, TimeSpan?)"/>
    public static Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Result self,
            EventDrivenGrain<TState, TEventBase> grain,
            Func<ICoordinatingStreamTransaction<TState, TEventBase>, Result> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
        => self.BindWithStreamTransaction(grain, t => Task.FromResult(f(t)), timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase}(Result, EventDrivenGrain{TState, TEventBase}, Action{ICoordinatingStreamTransaction{TState, TEventBase}}, TimeSpan?)"/>
    public static async Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Result self,
            EventDrivenGrain<TState, TEventBase> grain,
            Func<ICoordinatingStreamTransaction<TState, TEventBase>, Task<Result>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
    {
        if (self.IsFailure)
        {
            return Result.Failure<IStreamTransactionResponse>(self.Error);
        }
        try
        {
            await using var transaction = grain.BeginStreamTransaction(timeout);
            var result = await f(transaction);
            if (result.IsFailure)
            {
                return Result.Failure<IStreamTransactionResponse>(result.Error);
            }
            return await grain.SaveChanges(transaction);
        }
        catch (Exception ex)
        {
            return Result.Failure<IStreamTransactionResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(grain.Logger, ex));
        }
    }

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase}(Result, EventDrivenGrain{TState, TEventBase}, Action{ICoordinatingStreamTransaction{TState, TEventBase}}, TimeSpan?)"/>
    public static async Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Task<Result> self,
            EventDrivenGrain<TState, TEventBase> grain,
            Action<ICoordinatingStreamTransaction<TState, TEventBase>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
        => await (await self).BindWithStreamTransaction(grain, f, timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase}(Result, EventDrivenGrain{TState, TEventBase}, Action{ICoordinatingStreamTransaction{TState, TEventBase}}, TimeSpan?)"/>
    public static async Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Task<Result> self,
            EventDrivenGrain<TState, TEventBase> grain,
            Func<ICoordinatingStreamTransaction<TState, TEventBase>, Result> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
        => await (await self).BindWithStreamTransaction(grain, f, timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase}(Result, EventDrivenGrain{TState, TEventBase}, Action{ICoordinatingStreamTransaction{TState, TEventBase}}, TimeSpan?)"/>
    public static async Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase>(this Task<Result> self,
            EventDrivenGrain<TState, TEventBase> grain,
            Func<ICoordinatingStreamTransaction<TState, TEventBase>, Task<Result>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenState<TState, TEventBase>, new()
        where TEventBase : class
        => await (await self).BindWithStreamTransaction(grain, f, timeout);

    /// <summary>
    /// When <paramref name="self"/> succeeds, begins a stream transaction on <paramref name="grain"/>, runs
    /// <paramref name="f"/> against it and saves it; otherwise propagates the failure. Events applied through the
    /// transaction go to the grain's own stream. When participants are prepared, their events are saved atomically with
    /// the grain's own events. When <paramref name="f"/> fails or throws, or the save fails, every prepared participant
    /// is rolled back.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <typeparam name="TEventBase">The base type of events applied to the state.</typeparam>
    /// <typeparam name="TStatus">The state machine status type.</typeparam>
    /// <typeparam name="TCommand">The state machine trigger type.</typeparam>
    /// <param name="self">The source result.</param>
    /// <param name="grain">The grain that owns the transaction.</param>
    /// <param name="f">The operation to run against the transaction.</param>
    /// <param name="timeout">How long the transaction may take when participants are prepared; defaults to 30 seconds.</param>
    /// <returns>The result of saving the transaction, or the failure of the source result, the operation or the save.</returns>
    public static Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase, TStatus, TCommand>(this Result self,
            EventDrivenStateMachineGrain<TState, TEventBase, TStatus, TCommand> grain,
            Action<ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : StateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
        => self.BindWithStreamTransaction(grain, t => { f(t); return Task.FromResult(Result.Success()); }, timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase, TStatus, TCommand}(Result, EventDrivenStateMachineGrain{TState, TEventBase, TStatus, TCommand}, Action{ICoordinatingStateMachineStreamTransaction{TState, TEventBase, TStatus, TCommand}}, TimeSpan?)"/>
    public static Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase, TStatus, TCommand>(this Result self,
            EventDrivenStateMachineGrain<TState, TEventBase, TStatus, TCommand> grain,
            Func<ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>, Result> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : StateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
        => self.BindWithStreamTransaction(grain, t => Task.FromResult(f(t)), timeout);

    /// <inheritdoc cref="BindWithStreamTransaction{TState, TEventBase, TStatus, TCommand}(Result, EventDrivenStateMachineGrain{TState, TEventBase, TStatus, TCommand}, Action{ICoordinatingStateMachineStreamTransaction{TState, TEventBase, TStatus, TCommand}}, TimeSpan?)"/>
    public static async Task<Result<IStreamTransactionResponse>> BindWithStreamTransaction<TState, TEventBase, TStatus, TCommand>(this Result self,
            EventDrivenStateMachineGrain<TState, TEventBase, TStatus, TCommand> grain,
            Func<ICoordinatingStateMachineStreamTransaction<TState, TEventBase, TStatus, TCommand>, Task<Result>> f,
            TimeSpan? timeout = null)
        where TState : EventDrivenStateMachineState<TState, TEventBase, TStatus, TCommand>, new()
        where TEventBase : StateMachineEvent<TCommand>
        where TStatus : notnull
        where TCommand : notnull
    {
        if (self.IsFailure)
        {
            return Result.Failure<IStreamTransactionResponse>(self.Error);
        }
        try
        {
            await using var transaction = grain.BeginStreamTransaction(timeout);
            var result = await f(transaction);
            if (result.IsFailure)
            {
                return Result.Failure<IStreamTransactionResponse>(result.Error);
            }
            return await grain.SaveChanges(transaction);
        }
        catch (Exception ex)
        {
            return Result.Failure<IStreamTransactionResponse>(Result.Configuration.DefaultTryErrorHandlerWithLogging(grain.Logger, ex));
        }
    }
}
