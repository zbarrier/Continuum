using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T}(in Maybe{T}, Func{T})"/>
        public static async ValueTask<T> GetValueOrDefault<T>(this ValueTask<Maybe<T>> maybeTask, Func<T> defaultValue)
        {
            var maybe = await maybeTask;
            return maybe.GetValueOrDefault(defaultValue);
        }
        
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, K)"/>
        public static async ValueTask<K?> GetValueOrDefault<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, K> selector,
            K? defaultValue = default)
        {
            var maybe = await maybeTask;
            return maybe.GetValueOrDefault(selector, defaultValue);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, K> selector,
            Func<K> defaultValue)
        {
            var maybe = await maybeTask;
            return maybe.GetValueOrDefault(selector, defaultValue);
        }
    }
}
