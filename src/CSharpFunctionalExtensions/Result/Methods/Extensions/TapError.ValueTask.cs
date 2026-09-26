#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Func<ValueTask> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure)
            {
                await valueTask();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Func<ValueTask> valueTask)
        {
            Result result = await resultTask;

            if (result.IsFailure)
            {
                await valueTask();
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Func<Error, ValueTask> valueTask)
        {
            Result result = await resultTask;

            if (result.IsFailure)
            {
                await valueTask(result.Error);
            }

            return result;
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsFailure)
            {
                await valueTask(result.Error);
            }

            return result;
        }
    }
}
#endif
