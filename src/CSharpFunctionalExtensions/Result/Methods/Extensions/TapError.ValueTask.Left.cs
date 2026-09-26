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
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Action action)
        {
            Result<T> result = await resultTask;
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Action action)
        {
            Result result = await resultTask;
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> TapError<T>(this ValueTask<Result<T>> resultTask, Action<Error> action)
        {
            Result<T> result = await resultTask;
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> TapError(this ValueTask<Result> resultTask, Action<Error> action)
        {
            Result result = await resultTask;
            return result.TapError(action);
        }
    }
}
#endif
