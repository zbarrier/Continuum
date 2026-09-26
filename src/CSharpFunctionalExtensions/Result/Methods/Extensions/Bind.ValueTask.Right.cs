using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static ValueTask<Result<K>> Bind<T, K>(this Result<T> result, Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error).AsCompletedValueTask();

            return valueTask(result.Value);
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static ValueTask<Result<K>> Bind<K>(this Result result, Func<ValueTask<Result<K>>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error).AsCompletedValueTask();

            return valueTask();
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static ValueTask<Result> Bind<T>(this Result<T> result, Func<T, ValueTask<Result>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure(result.Error).AsCompletedValueTask();

            return valueTask(result.Value);
        }

        /// <summary>
        ///     Selects result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static ValueTask<Result> Bind(this Result result, Func<ValueTask<Result>> valueTask)
        {
            if (result.IsFailure)
                return result.AsCompletedValueTask();

            return valueTask();
        }
    }
}