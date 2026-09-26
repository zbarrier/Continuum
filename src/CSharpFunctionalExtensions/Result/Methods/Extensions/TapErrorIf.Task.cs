using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Task<Result> resultTask, bool condition, Func<Task> func)
        {
            if (condition)
            {
                return resultTask.TapError(func);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Task<Result> resultTask, bool condition, Func<Error, Task> func)
        {
            if (condition)
            {
                return resultTask.TapError(func);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, bool condition, Func<Task> func)
        {
            if (condition)
            {
                return resultTask.TapError(func);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, bool condition, Func<Error, Task> func)
        {
            if (condition)
            {
                return resultTask.TapError(func);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapErrorIf(this Task<Result> resultTask, Func<Error, bool> predicate, Func<Task> func)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(func).DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapErrorIf(this Task<Result> resultTask, Func<Error, bool> predicate, Func<Error, Task> func)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(func).DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Task> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(func).DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, Task> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(func).DefaultAwait();
            }

            return result;
        }
    }
}
