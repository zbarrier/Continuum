using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success and the condition is true. Returns the calling result.
        ///     If there is an exception, returns a new failure Result.
        /// </summary>
        public static async Task<Result> TapIfTry(this Task<Result> resultTask, bool condition, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

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
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, bool condition, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

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
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

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
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

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
        public static async Task<Result<T>> TapIfTry<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task> func, Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask.DefaultAwait();

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
