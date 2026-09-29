using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T}(in Result{T}, Func{T})"/>
        public static async Task<T> GetValueOrDefault<T>(this Task<Result<T>> resultTask, Func<Task<T>> defaultValue)
        {
            var result = await resultTask.DefaultAwait();
            return await result.GetValueOrDefault(defaultValue).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, K)"/>
        public static async Task<K?> GetValueOrDefault<T, K>(this Task<Result<T>> resultTask, Func<T, Task<K>> selector,
            K? defaultValue = default)
        {
            var result = await resultTask.DefaultAwait();
            return await result.GetValueOrDefault(selector, defaultValue).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Task<Result<T>> resultTask, Func<T, Task<K>> selector,
            Func<Task<K>> defaultValue)
        {
            var result = await resultTask.DefaultAwait();
            return await result.GetValueOrDefault(selector, defaultValue).DefaultAwait();
        }
    }
}
