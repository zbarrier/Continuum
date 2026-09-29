using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T}(in Result{T}, Func{T})"/>
        public static async ValueTask<T> GetValueOrDefault<T>(this Result<T> result, Func<ValueTask<T>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask();

            return result.Value;
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this Result<T> result, Func<T, K> selector,
            Func<ValueTask<K>> valueTask)
        {
            if (result.IsFailure)
                return await valueTask();

            return selector(result.Value);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, K)"/>
        public static async ValueTask<K?> GetValueOrDefault<T, K>(this Result<T> result, Func<T, ValueTask<K>> valueTask,
            K? defaultValue = default)
        {
            if (result.IsFailure)
                return defaultValue;

            return await valueTask(result.Value);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.GetValueOrDefault{T, K}(in Result{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this Result<T> result, Func<T, ValueTask<K>> valueTask,
            Func<ValueTask<K>> defaultValue)
        {
            if (result.IsFailure)
                return await defaultValue();

            return await valueTask(result.Value);
        }
    }
}
