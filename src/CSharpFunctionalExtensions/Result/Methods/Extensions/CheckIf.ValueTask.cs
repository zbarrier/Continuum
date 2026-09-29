using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, bool, Func{T, Result})"/>
        public static ValueTask<Result<T>> CheckIf<T>(this ValueTask<Result<T>> resultTask, bool condition,
            Func<T, ValueTask<Result>> valueTask)
        {
            if (condition)
                return resultTask.Check(valueTask);
            else
                return resultTask;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, bool, Func{T, Result{K}})"/>
        public static ValueTask<Result<T>> CheckIf<T, K>(this ValueTask<Result<T>> resultTask, bool condition,
            Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (condition)
                return resultTask.Check(valueTask);
            else
                return resultTask;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, Func{T, bool}, Func{T, Result})"/>
        public static async ValueTask<Result<T>> CheckIf<T>(this ValueTask<Result<T>> resultTask,
            Func<T, bool> predicate, Func<T, ValueTask<Result>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(valueTask);
            else
                return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, Func{T, bool}, Func{T, Result{K}})"/>
        public static async ValueTask<Result<T>> CheckIf<T, K>(this ValueTask<Result<T>> resultTask,
            Func<T, bool> predicate, Func<T, ValueTask<Result<K>>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(valueTask);
            else
                return result;
        }
    }
}
