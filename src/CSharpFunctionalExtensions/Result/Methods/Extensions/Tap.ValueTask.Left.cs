using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result> Tap(this ValueTask<Result> resultTask, Action action)
        {
            Result result = await resultTask;
            return result.Tap(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> Tap<T>(this ValueTask<Result<T>> resultTask, Action action)
        {
            Result<T> result = await resultTask;
            return result.Tap(action);
        }

        /// <summary>
        ///     Executes the given action if the calling result is a success. Returns the calling result.
        /// </summary>
        public static async ValueTask<Result<T>> Tap<T>(this ValueTask<Result<T>> resultTask, Action<T> action)
        {
            Result<T> result = await resultTask;
            return result.Tap(action);
        }
    }
}
