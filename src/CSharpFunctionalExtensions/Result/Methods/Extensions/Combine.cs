using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Result Combine(this IEnumerable<Result> results)
            => Result.Combine(results);

        public static Result<IEnumerable<T>> Combine<T>(this IEnumerable<Result<T>> results)
        {
            results = results.ToList();
            Result result = Result.Combine(results);

            return result.IsSuccess
                ? Result.Success(results.Select(e => e.Value))
                : Result.Failure<IEnumerable<T>>(result.Error);
        }

        public static Result<K> Combine<T, K>(this IEnumerable<Result<T>> results, Func<IEnumerable<T>, K> composer)
        {
            Result<IEnumerable<T>> result = results.Combine();

            return result.IsSuccess
                ? Result.Success(composer(result.Value))
                : Result.Failure<K>(result.Error);
        }
    }
}
