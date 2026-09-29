using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, bool, Func{T, Result})"/>
        public static Task<Result<T>> CheckIf<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Task<Result>> func)
        {
            if (condition)
                return resultTask.Check(func);
            else
                return resultTask;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, bool, Func{T, Result{K}})"/>
        public static Task<Result<T>> CheckIf<T, K>(this Task<Result<T>> resultTask, bool condition, Func<T, Task<Result<K>>> func)
        {
            if (condition)
                return resultTask.Check(func);
            else
                return resultTask;
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T}(Result{T}, Func{T, bool}, Func{T, Result})"/>
        public static async Task<Result<T>> CheckIf<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task<Result>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(func).DefaultAwait();
            else
                return result;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.CheckIf{T, K}(Result{T}, Func{T, bool}, Func{T, Result{K}})"/>
        public static async Task<Result<T>> CheckIf<T, K>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Task<Result<K>>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(func).DefaultAwait();
            else
                return result;
        }
    }
}
