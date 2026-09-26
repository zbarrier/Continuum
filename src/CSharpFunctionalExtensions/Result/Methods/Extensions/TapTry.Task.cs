using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result> TapTry(this Task<Result> resultTask, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    await func().DefaultAwait();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return Result.Failure(error);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapTry<T>(this Task<Result<T>> resultTask, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();
            
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    await func().DefaultAwait();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapTry<T>(this Task<Result<T>> resultTask, Func<T, Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess)
                    await func(result.Value).DefaultAwait();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }
    }
}
