using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapError<T>(this Task<Result<T>> resultTask, Action action)
        {
            Result<T> result = await resultTask.DefaultAwait();
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapError(this Task<Result> resultTask, Action action)
        {
            Result result = await resultTask.DefaultAwait();
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async Task<Result<T>> TapError<T>(this Task<Result<T>> resultTask, Action<Error> action)
        {
            Result<T> result = await resultTask.DefaultAwait();
            return result.TapError(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a failure. Returns the calling result.
        /// </summary>
        public static async Task<Result> TapError(this Task<Result> resultTask, Action<Error> action)
        {
            Result result = await resultTask.DefaultAwait();
            return result.TapError(action);
        }
    }
}
