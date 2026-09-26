using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Attempts to execute the supplied action. Returns a Result indicating whether the action executed successfully.
    /// </summary>
    public static async ValueTask<Result> Try(Func<ValueTask> action, Func<Exception, Error> errorHandler = null)
    {
        errorHandler ??= Configuration.DefaultTryErrorHandler;

        try
        {
            await action().DefaultAwait();
            return Success();
        }
        catch (Exception exc)
        {
            Error error = errorHandler(exc);
            return Failure(error);
        }
    }

    /// <summary>
    ///     Attempts to execute the supplied function. Returns a Result indicating whether the function executed successfully.
    ///     If the function executed successfully, the result contains its return value.
    /// </summary>
    public static async ValueTask<Result<T>> Try<T>(Func<ValueTask<T>> func, Func<Exception, Error> errorHandler = null)
    {
        errorHandler ??= Configuration.DefaultTryErrorHandler;

        try
        {
            var result = await func().DefaultAwait();
            return Success(result);
        }
        catch (Exception exc)
        {
            Error error = errorHandler(exc);
            return Failure<T>(error);
        }
    }
}