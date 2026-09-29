using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T}(in Result{T}, Func{T})"/>
        public static async Task<T> GetValueOrDefault<T>(this Result<T> result, Func<Task<T>> defaultValue)
        {
            if (result.IsFailure)
                return await defaultValue().DefaultAwait();

            return result.Value;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Result<T> result, Func<T, K> selector,
            Func<Task<K>> defaultValue)
        {
            if (result.IsFailure)
                return await defaultValue().DefaultAwait();

            return selector(result.Value);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, K)"/>
        public static async Task<K?> GetValueOrDefault<T, K>(this Result<T> result, Func<T, Task<K>> selector,
            K? defaultValue = default)
        {
            if (result.IsFailure)
                return defaultValue;

            return await selector(result.Value).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Result<T> result, Func<T, Task<K>> selector,
            Func<Task<K>> defaultValue)
        {
            if (result.IsFailure)
                return await defaultValue().DefaultAwait();

            return await selector(result.Value).DefaultAwait();
        }
    }
}
