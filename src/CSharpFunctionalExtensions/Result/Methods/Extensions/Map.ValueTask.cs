using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K>(this ValueTask<Result<T>> resultTask, Func<T, ValueTask<K>> valueTask)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(result.Value).DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<T, K, TContext>(
            this ValueTask<Result<T>> resultTask,
            Func<T, TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(result.Value, context).DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<K>(this ValueTask<Result> resultTask, Func<ValueTask<K>> valueTask)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask().DefaultAwait();

            return Result.Success(value);
        }

        /// <summary>
        ///     Creates a new result from the return value of a given valueTask action. If the calling Result is a failure, a new failure result is returned instead.
        /// </summary>
        public static async ValueTask<Result<K>> Map<K, TContext>(
            this ValueTask<Result> resultTask,
            Func<TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return Result.Failure<K>(result.Error);

            K value = await valueTask(context).DefaultAwait();

            return Result.Success(value);
        }
    }
}
