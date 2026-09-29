using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T}(in Maybe{T}, Func{T})"/>
        public static async Task<T> GetValueOrDefault<T>(this Maybe<T> maybe, Func<Task<T>> defaultValue)
        {
            if (maybe.HasNoValue)
                return await defaultValue().DefaultAwait();

            return maybe.GetValueOrThrow();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, K> selector,
            Func<Task<K>> defaultValue)
        {
            if (maybe.HasNoValue)
                return await defaultValue().DefaultAwait();

            return selector(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, K)"/>
        public static async Task<K?> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, Task<K>> selector,
            K? defaultValue = default)
        {
            if (maybe.HasNoValue)
                return defaultValue;

            return await selector(maybe.GetValueOrThrow()).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async Task<K> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, Task<K>> selector,
            Func<Task<K>> defaultValue)
        {
            if (maybe.HasNoValue)
                return await defaultValue().DefaultAwait();

            return await selector(maybe.GetValueOrThrow()).DefaultAwait();
        }
    }
}
