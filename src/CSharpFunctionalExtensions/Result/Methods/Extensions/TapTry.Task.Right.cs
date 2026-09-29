using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result> TapTry(this Result result, Func<Task> func, Func<Exception, Error>? errorHandler = null)
        {
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
        public static async Task<Result<T>> TapTry<T>(this Result<T> result, Func<Task> func, Func<Exception, Error>? errorHandler = null)
        {
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
        public static async Task<Result<T>> TapTry<T>(this Result<T> result, Func<T, Task> func, Func<Exception, Error>? errorHandler = null)
        {
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
