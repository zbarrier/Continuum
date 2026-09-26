#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>
        public static async ValueTask<Result<K>> MapTry<T, K>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<K>> valueTask,
            Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask;
            return await result.MapTry(valueTask, errorHandler);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>
        public static async ValueTask<Result<K>> MapTry<K>(this ValueTask<Result> resultTask, Func<ValueTask<K>> valueTask,
            Func<Exception, Error> errorHandler = null)
        {
            var result = await resultTask;
            return await result.MapTry(valueTask, errorHandler);
        }
    }
}
#endif