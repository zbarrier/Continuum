using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Result<K> Map<T, K>(this Result<T> result, Func<T, K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func(result.Value));
        }

        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Result<K> Map<T, K, TContext>(
            this Result<T> result,
            Func<T, TContext, K> func,
            TContext context
        )
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func(result.Value, context));
        }

        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Result<K> Map<K>(this Result result, Func<K> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func());
        }

        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Result<K> Map<K, TContext>(
            this Result result,
            Func<TContext, K> func,
            TContext context
        )
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            return Result.Success(func(context));
        }

        //public static Result<Maybe<K>> Map<T, K>(
        //    this Result<Maybe<T>> result,
        //    Func<T, K> func)
        //{
        //    if (result.IsFailure)
        //        return Result.Failure<Maybe<K>>(result.Error);

        //    return Result.Success<Maybe<K>>(result.Value.Map(func));
        //}
    }
}
