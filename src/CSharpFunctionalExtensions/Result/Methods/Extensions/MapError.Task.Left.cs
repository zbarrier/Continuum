using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError(this Task<Result> resultTask, Func<Error, Error> errorFactory)
        {
            var result = await resultTask.DefaultAwait();
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = errorFactory(result.Error);
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError<TContext>(
            this Task<Result> resultTask,
            Func<Error, TContext, Error> errorFactory,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = errorFactory(result.Error, context);
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T>(this Task<Result<T>> resultTask, Func<Error, Error> errorFactory)
        {
            var result = await resultTask.DefaultAwait();
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = errorFactory(result.Error);
            return Result.Failure<T>(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T, TContext>(
            this Task<Result<T>> resultTask,
            Func<Error, TContext, Error> errorFactory,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = errorFactory(result.Error, context);
            return Result.Failure<T>(error);
        }
    }
}