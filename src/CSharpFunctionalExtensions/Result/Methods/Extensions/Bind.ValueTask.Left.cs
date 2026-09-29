using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    /// <summary>
    ///     Async extension methods for <see cref="Result"/> and <see cref="Result{T}"/> where the source result is asynchronous and the delegate is synchronous.
    /// </summary>
    public static partial class AsyncResultExtensionsLeftOperand
    {
        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Bind<T, K>(this ValueTask<Result<T>> resultTask, Func<T, Result<K>> valueTask)
        {
            Result<T> result = await resultTask;
            return result.Bind(valueTask);
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Bind<K>(this ValueTask<Result> resultTask, Func<Result<K>> valueTask)
        {
            Result result = await resultTask;
            return result.Bind(valueTask);
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result> Bind<T>(this ValueTask<Result<T>> resultTask, Func<T, Result> valueTask)
        {
            Result<T> result = await resultTask;
            return result.Bind(valueTask);
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result> Bind(this ValueTask<Result> resultTask, Func<Result> valueTask)
        {
            Result result = await resultTask;
            return result.Bind(valueTask);
        }
    }
}
