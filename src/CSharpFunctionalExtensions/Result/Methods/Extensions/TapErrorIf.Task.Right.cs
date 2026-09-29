using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Result result, bool condition, Func<Task> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Result result, bool condition, Func<Error, Task> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Result<T> result, bool condition, Func<Task> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Result<T> result, bool condition, Func<Error, Task> func)
        {
            if (condition)
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Result result, Func<Error, bool> predicate, Func<Task> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result> TapErrorIf(this Result result, Func<Error, bool> predicate, Func<Error, Task> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Func<Task> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Task<Result<T>> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Func<Error, Task> func)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(func);
            }

            return Task.FromResult(result);
        }
    }
}
