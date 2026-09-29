using System;

using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Creates a new result from the return value of a given function if the condition is true. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="condition">When <see langword="false"/>, <paramref name="result"/> is returned unchanged.</param>
        /// <param name="func">The mapping function applied to the success value.</param>
        /// <returns>The mapped result, or <paramref name="result"/> when the condition is false or the result is a failure.</returns>
        public static Result<T> MapIf<T>(this Result<T> result, bool condition, Func<T, T> func)
        {
            if (!condition)
            {
                return result;
            }

            return result.Map(func);
        }

        /// <inheritdoc cref="MapIf{T}(Result{T}, bool, Func{T, T})"/>
        public static Result<T> MapIf<T, TContext>(
            this Result<T> result,
            bool condition,
            Func<T, TContext, T> func,
            TContext context
        )
        {
            if (!condition)
            {
                return result;
            }

            return result.Map(func, context);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given function if the predicate is true. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="predicate">Evaluated against the success value; when it returns <see langword="false"/>, <paramref name="result"/> is returned unchanged.</param>
        /// <param name="func">The mapping function applied to the success value.</param>
        /// <returns>The mapped result, or <paramref name="result"/> when the predicate is false or the result is a failure.</returns>
        public static Result<T> MapIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, T> func)
        {
            if (!result.IsSuccess || !predicate(result.Value))
            {
                return result;
            }

            return result.Map(func);
        }

        /// <inheritdoc cref="MapIf{T}(Result{T}, Func{T, bool}, Func{T, T})"/>
        public static Result<T> MapIf<T, TContext>(
            this Result<T> result,
            Func<T, TContext, bool> predicate,
            Func<T, TContext, T> func,
            TContext context
        )
        {
            if (!result.IsSuccess || !predicate(result.Value, context))
            {
                return result;
            }

            return result.Map(func, context);
        }
    }
}
