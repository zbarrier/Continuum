namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class AsyncResultExtensionsBothOperands
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result> TapTry(this ValueTask<Result> resultTask, Func<ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
        var result = await resultTask.DefaultAwait();

        errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
        
        try
        {
            if (result.IsSuccess)
            {
                await func().DefaultAwait();
            }
            return result;
        }
        catch (Exception exc)
        {
            var error = errorHandler(exc);
            return Result.Failure(error);
        }
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapTry<T>(this ValueTask<Result<T>> resultTask, Func<ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
        var result = await resultTask.DefaultAwait();
        
        errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
        
        try
        {
            if (result.IsSuccess)
            {
                await func().DefaultAwait();
            }
            return result;
        }
        catch (Exception exc)
        {
            var error = errorHandler(exc);
            return new Result<T>(true, error, default);
        }
    }

    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result<T>> TapTry<T>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
        var result = await resultTask.DefaultAwait();

        errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
        
        try
        {
            if (result.IsSuccess)
            {
                await func(result.Value).DefaultAwait();
            }
            return result;
        }
        catch (Exception exc)
        {
            var error = errorHandler(exc);
            return new Result<T>(true, error, default);
        }
    }
}
