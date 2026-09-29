using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T}(in Result{T}, Func{T})"/>
        public static async ValueTask<T> GetValueOrDefault<T>(this ValueTask<Result<T>> resultTask, Func<T> defaultValue)
        {
            var result = await resultTask;
            return result.GetValueOrDefault(defaultValue);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, K)"/>
        public static async ValueTask<K?> GetValueOrDefault<T, K>(this ValueTask<Result<T>> resultTask, Func<T, K> selector,
            K? defaultValue = default)
        {
            var result = await resultTask;
            return result.GetValueOrDefault(selector, defaultValue);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this ValueTask<Result<T>> resultTask, Func<T, K> selector,
            Func<K> defaultValue)
        {
            var result = await resultTask;
            return result.GetValueOrDefault(selector, defaultValue);
        }
    }
}
