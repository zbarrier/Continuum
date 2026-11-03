using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K>(this Result<T> result, Func<T, ValueTask<K>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(result.Value).DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K, TContext>(
            this Result<T> result,
            Func<T, TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(result.Value, context).DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K>(this Result<T> result, Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            Result<K> value = await valueTask(result.Value).DefaultAwait();
            return value;
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K, TContext>(
            this Result<T> result,
            Func<T, TContext, ValueTask<Result<K>>> valueTask,
            TContext context
        )
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            Result<K> value = await valueTask(result.Value, context).DefaultAwait();
            return value;
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<K>(this Result result, Func<ValueTask<K>> valueTask)
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask().DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<K, TContext>(
            this Result result,
            Func<TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(context).DefaultAwait();

            return Result.Success(value);
        }
    }
}
