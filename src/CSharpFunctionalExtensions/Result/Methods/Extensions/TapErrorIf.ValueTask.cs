using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this ValueTask<Result> resultTask, bool condition, Func<ValueTask> valueTask)
        {
            if (condition)
            {
                return resultTask.TapError(valueTask);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this ValueTask<Result> resultTask, bool condition, Func<Error, ValueTask> valueTask)
        {
            if (condition)
            {
                return resultTask.TapError(valueTask);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<ValueTask> valueTask)
        {
            if (condition)
            {
                return resultTask.TapError(valueTask);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<Error, ValueTask> valueTask)
        {
            if (condition)
            {
                return resultTask.TapError(valueTask);
            }

            return resultTask;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapErrorIf(this ValueTask<Result> resultTask, Func<Error, bool> predicate, Func<ValueTask> valueTask)
        {
            Result result = await resultTask;

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(valueTask);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapErrorIf(this ValueTask<Result> resultTask, Func<Error, bool> predicate, Func<Error, ValueTask> valueTask)
        {
            Result result = await resultTask;

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(valueTask);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapErrorIf<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<ValueTask> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(valueTask);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapErrorIf<T>(this ValueTask<Result<T>> resultTask, Func<Error, bool> predicate, Func<Error, ValueTask> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure && predicate(result.Error))
            {
                return await result.TapError(valueTask);
            }

            return result;
        }
    }
}