namespace Continuum.CSharpFunctionalExtensions.ValueTasks;

public static partial class AsyncResultExtensionsRightOperand
{
    /// <summary>
    ///     Executes the given action if the calling result is a success. Returns the calling result.
    ///     If there is an exception, returns a new failure Result.
    /// </summary>
    public static async ValueTask<Result> TapTry(this Result result, Func<ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
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
    public static async ValueTask<Result<T>> TapTry<T>(this Result<T> result, Func<ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
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
    public static async ValueTask<Result<T>> TapTry<T>(this Result<T> result, Func<T, ValueTask> func, Func<Exception, Error>? errorHandler = null)
    {
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
