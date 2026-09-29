namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class ResultExtensions
{
    /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry(Result, Action, Func{Exception, Error})"/>
    public static async ValueTask<Result> OnSuccessTry(this ValueTask<Result> task, Func<ValueTask> func,
        Func<Exception, Error>? errorHandler = null)
    {
        var result = await task.DefaultAwait();
        return await result.OnSuccessTry(func, errorHandler).DefaultAwait();
    }

    /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry{T}(Result{T}, Action{T}, Func{Exception, Error})"/>
    public static async ValueTask<Result> OnSuccessTry<T>(this ValueTask<Result<T>> task, Func<T, ValueTask> func,
        Func<Exception, Error>? errorHandler = null)
    {
        var result = await task.DefaultAwait();
        return await result.OnSuccessTry(func, errorHandler).DefaultAwait();
    }
}
