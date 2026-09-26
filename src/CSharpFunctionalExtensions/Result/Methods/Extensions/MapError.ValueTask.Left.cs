#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result> MapError(this ValueTask<Result> resultTask, Func<Error, Error> errorFactory)
        {
            var result = await resultTask;
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = errorFactory(result.Error);
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result> MapError<TContext>(
            this ValueTask<Result> resultTask,
            Func<Error, TContext, Error> errorFactory,
            TContext context
        )
        {
            var result = await resultTask;
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            var error = errorFactory(result.Error, context);
            return Result.Failure(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result<T>> MapError<T>(this ValueTask<Result<T>> resultTask, Func<Error, Error> errorFactory)
        {
            var result = await resultTask;
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = errorFactory(result.Error);
            return Result.Failure<T>(error);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result<T>> MapError<T, TContext>(
            this ValueTask<Result<T>> resultTask,
            Func<Error, TContext, Error> errorFactory,
            TContext context
        )
        {
            var result = await resultTask;
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            var error = errorFactory(result.Error, context);
            return Result.Failure<T>(error);
        }
    }
}
#endif