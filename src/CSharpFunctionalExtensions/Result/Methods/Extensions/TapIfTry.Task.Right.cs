using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result> TapIfTry(this Result result, bool condition, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (condition && result.IsSuccess)
                    await func();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return Result.Failure(error);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Result<T> result, bool condition, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (condition && result.IsSuccess)
                    await func();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Result<T> result, bool condition, Func<T, Task> func, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (condition && result.IsSuccess)
                    await func(result.Value);

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Result<T> result, Func<T, bool> predicate, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess && predicate(result.Value) && result.IsSuccess)
                    await func();

                return result;
            }
            catch (Exception exc)
            {
                Error error = errorHandler(exc);
                return new Result<T>(true, error, default);
            }
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success and the predicate is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result<T>> TapIfTry<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Task> func, Func<Exception, Error> errorHandler = null)
        {
            errorHandler ??= Result.Configuration.DefaultTryErrorHandler;
            
            try
            {
                if (result.IsSuccess && predicate(result.Value))
                    await func(result.Value);

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
