using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Result.Combine(IEnumerable{Result})"/>
        public static Result Combine(this IEnumerable<Result> results)
            => Result.Combine(results);

        /// <summary>
        ///     Combines a sequence of results. Returns a success containing all values when every result succeeds; otherwise a failure with the combined error.
        /// </summary>
        /// <typeparam name="T">The type of the result values.</typeparam>
        /// <param name="results">The results to combine. The sequence is enumerated once.</param>
        /// <returns>A success containing the values in order, or a failure.</returns>
        public static Result<IEnumerable<T>> Combine<T>(this IEnumerable<Result<T>> results)
        {
            results = results.ToList();
            Result result = Result.Combine(results);

            return result.IsSuccess
                ? Result.Success(results.Select(e => e.Value))
                : Result.Failure<IEnumerable<T>>(result.Error);
        }

        /// <summary>
        ///     Combines a sequence of results and, when all succeed, composes their values into a single value.
        /// </summary>
        /// <typeparam name="T">The type of the result values.</typeparam>
        /// <typeparam name="K">The type of the composed value.</typeparam>
        /// <param name="results">The results to combine.</param>
        /// <param name="composer">Composes the success values into the final value.</param>
        /// <returns>A success containing the composed value, or a failure.</returns>
        public static Result<K> Combine<T, K>(this IEnumerable<Result<T>> results, Func<IEnumerable<T>, K> composer)
        {
            Result<IEnumerable<T>> result = results.Combine();

            return result.IsSuccess
                ? Result.Success(composer(result.Value))
                : Result.Failure<K>(result.Error);
        }
    }
}
