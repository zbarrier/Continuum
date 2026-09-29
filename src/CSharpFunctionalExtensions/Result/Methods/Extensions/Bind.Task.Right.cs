using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Async extension methods for <see cref="Result"/> and <see cref="Result{T}"/> where the source result is synchronous and the delegate is asynchronous.
    /// </summary>
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Task<Result<K>> Bind<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error).AsCompletedTask();

            return func(result.Value);
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Task<Result<K>> Bind<K>(this Result result, Func<Task<Result<K>>> func)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error).AsCompletedTask();

            return func();
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Task<Result> Bind<T>(this Result<T> result, Func<T, Task<Result>> func)
        {
            if (result.IsFailure)
                return Result.Failure(result.Error).AsCompletedTask();

            return func(result.Value);
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static Task<Result> Bind(this Result result, Func<Task<Result>> func)
        {
            if (result.IsFailure)
                return result.AsCompletedTask();

            return func();
        }
    }
}
