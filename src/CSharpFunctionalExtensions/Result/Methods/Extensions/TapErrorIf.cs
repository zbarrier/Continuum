using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result TapErrorIf(this Result result, bool condition, Action action)
        {
            if (condition)
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result TapErrorIf(this Result result, bool condition, Action<Error> action)
        {
            if (condition)
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result<T> TapErrorIf<T>(this Result<T> result, bool condition, Action action)
        {
            if (condition)
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result<T> TapErrorIf<T>(this Result<T> result, bool condition, Action<Error> action)
        {
            if (condition)
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result TapErrorIf(this Result result, Func<Error, bool> predicate, Action action)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result TapErrorIf(this Result result, Func<Error, bool> predicate, Action<Error> action)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result<T> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Action action)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure and condition is true. Returns the calling result.
        /// </summary>
        public static Result<T> TapErrorIf<T>(this Result<T> result, Func<Error, bool> predicate, Action<Error> action)
        {
            if (result.IsFailure && predicate(result.Error))
            {
                return result.TapError(action);
            }

            return result;
        }
    }
}
