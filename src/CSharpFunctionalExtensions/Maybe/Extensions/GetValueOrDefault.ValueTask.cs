using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T}(in Maybe{T}, Func{T})"/>
        public static async ValueTask<T> GetValueOrDefault<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask<T>> defaultValue)
        {
            var maybe = await maybeTask;
            return await maybe.GetValueOrDefault(defaultValue);
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, K)"/>
        public static async ValueTask<K?> GetValueOrDefault<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<K>> selector,
            K? defaultValue = default)
        {
            var maybe = await maybeTask;
            return await maybe.GetValueOrDefault(selector, defaultValue);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<K>> selector,
            Func<ValueTask<K>> defaultValue)
        {
            var maybe = await maybeTask;
            return await maybe.GetValueOrDefault(selector, defaultValue);
        }
    }
}
