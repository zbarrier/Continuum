using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Task<Result> resultTask, bool condition, Action action)
        {
            if (condition)
            {
                return resultTask.TapError(action);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Task<Result> resultTask, bool condition, Action<Error> action)
        {
            if (condition)
            {
                return resultTask.TapError(action);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, bool condition, Action action)
        {
            if (condition)
            {
                return resultTask.TapError(action);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, bool condition, Action<Error> action)
        {
            if (condition)
            {
                return resultTask.TapError(action);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapErrorIf(this Task<Result> resultTask, Func<Error, bool> predicate, Action action)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapErrorIf(this Task<Result> resultTask, Func<Error, bool> predicate, Action<Error> action)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Action action)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapErrorIf<T>(this Task<Result<T>> resultTask, Func<Error, bool> predicate, Action<Error> action)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }
    }
}
