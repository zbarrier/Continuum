using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this Result result, bool condition, Func<ValueTask> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this Result result, bool condition, Func<Error, ValueTask> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this Result<T> result, bool condition, Func<ValueTask> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this Result<T> result, bool condition, Func<Error, ValueTask> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this Result result, Func<Error, bool> predicate, Func<ValueTask> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result> TapErrorIf(this Result result, Func<Error, bool> predicate, Func<Error, ValueTask> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Func<ValueTask> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static ValueTask<Result<T>> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Func<Error, ValueTask> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return result.AsCompletedValueTask();
        }
    }
}
