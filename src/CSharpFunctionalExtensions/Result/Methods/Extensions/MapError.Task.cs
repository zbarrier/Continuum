using System;
using System.Threading.Tasks;

using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError(this Task<Result> resultTask, Func<Error, Task<Error>> errorFactory)
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapError(errorFactory).DefaultAwait();
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result> MapError<TContext>(
            this Task<Result> resultTask,
            Func<Error, TContext, Task<Error>> errorFactory,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapError(errorFactory, context).DefaultAwait();
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T>(this Task<Result<T>> resultTask, Func<Error, Task<Error>> errorFactory)
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapError(errorFactory).DefaultAwait();
        }

        /// <summary>
        ///     If the calling Result is a success, a new success result is returned. Otherwise, creates a new failure result from the return value of a given function.
        /// </summary>
        public static async Task<Result<T>> MapError<T, TContext>(
            this Task<Result<T>> resultTask,
            Func<Error, TContext, Task<Error>> errorFactory,
            TContext context
        )
        {
            var result = await resultTask.DefaultAwait();
            return await result.MapError(errorFactory, context).DefaultAwait();
        }
    }
}