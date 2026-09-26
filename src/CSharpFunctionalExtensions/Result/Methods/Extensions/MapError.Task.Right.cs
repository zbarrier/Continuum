using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError(this Result result, Func<Error, Task<Error>> errorFactory)
        {
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = await errorFactory(result.Error).DefaultAwait();
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError<TContext>(
            this Result result,
            Func<Error, TContext, Task<Error>> errorFactory,
            TContext context
        )
        {
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = await errorFactory(result.Error, context).DefaultAwait();
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T>(this Result<T> result, Func<Error, Task<Error>> errorFactory)
        {
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = await errorFactory(result.Error).DefaultAwait();
            return Result.Failure<T>(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T, TContext>(
            this Result<T> result,
            Func<Error, TContext, Task<Error>> errorFactory,
            TContext context
        )
        {
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = await errorFactory(result.Error, context).DefaultAwait();
            return Result.Failure<T>(error);
        }
    }
}