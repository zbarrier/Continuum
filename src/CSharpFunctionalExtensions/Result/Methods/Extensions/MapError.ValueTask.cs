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
        public static async ValueTask<Result> MapError(this ValueTask<Result> resultTask, Func<Error, ValueTask<Error>> errorFactory)
        {
            var result = await resultTask;
            return await result.MapError(errorFactory);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result> MapError<TContext>(
            this ValueTask<Result> resultTask,
            Func<Error, TContext, ValueTask<Error>> errorFactory,
            TContext context
        )
        {
            var result = await resultTask;
            return await result.MapError(errorFactory, context);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result<T>> MapError<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<Error>> errorFactory)
        {
            var result = await resultTask;
            return await result.MapError(errorFactory);
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given valueTask action.
        /// </summary>
        public static async ValueTask<Result<T>> MapError<T, TContext>(
            this ValueTask<Result<T>> resultTask,
            Func<Error, TContext, ValueTask<Error>> errorFactory,
            TContext context
        )
        {
            var result = await resultTask;
            return await result.MapError(errorFactory, context);
        }
    }
}
#endif