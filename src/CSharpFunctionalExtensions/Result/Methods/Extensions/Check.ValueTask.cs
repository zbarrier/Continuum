#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     If the calling result is a success, the given valueTask action is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static async ValueTask<Result<T>> Check<T>(this ValueTask<Result<T>> resultTask,
            Func<T, ValueTask<Result>> valueTask)
        {
            Result<T> result = await resultTask;
            return await result.Bind(valueTask).Map(() => result.Value);
        }

        /// <summary>
        ///     If the calling result is a success, the given valueTask action is executed and its Result is checked. If this Result is a failure, it is returned. Otherwise, the calling result is returned.
        /// </summary>
        public static async ValueTask<Result<T>> Check<T, K>(this ValueTask<Result<T>> resultTask,
            Func<T, ValueTask<Result<K>>> valueTask)
        {
            Result<T> result = await resultTask;
            return await result.Bind(valueTask).Map(_ => result.Value);
        }
    }
}
#endif