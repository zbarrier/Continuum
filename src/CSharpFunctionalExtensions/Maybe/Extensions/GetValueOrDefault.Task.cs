using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T}(in Maybe{T}, Func{T})"/>
        public static async Task<T> GetValueOrDefault<T>(this Task<Maybe<T>> maybeTask, Func<Task<T>> defaultValue)
        {
            var maybe = await maybeTask.DefaultAwait();
            return await maybe.GetValueOrDefault(defaultValue).DefaultAwait();
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, K)"/>
        public static async Task<K?> GetValueOrDefault<T, K>(this Task<Maybe<T>> maybeTask, Func<T, Task<K>> selector,
            K? defaultValue = default)
        {
            var maybe = await maybeTask.DefaultAwait();
            return await maybe.GetValueOrDefault(selector, defaultValue).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Task<Maybe<T>> maybeTask, Func<T, Task<K>> selector,
            Func<Task<K>> defaultValue)
        {
            var maybe = await maybeTask.DefaultAwait();
            return await maybe.GetValueOrDefault(selector, defaultValue).DefaultAwait();
        }
    }
}
