using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Func<ValueTask> valueTask)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
            {
                await valueTask().DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Func<ValueTask> valueTask)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
            {
                await valueTask().DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Func<Error, ValueTask> valueTask)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
            {
                await valueTask(result.Error).DefaultAwait();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask> valueTask)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
            {
                await valueTask(result.Error).DefaultAwait();
            }

            return result;
        }
    }
}
