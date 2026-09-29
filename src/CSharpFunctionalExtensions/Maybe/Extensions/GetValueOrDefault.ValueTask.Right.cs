using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T}(in Maybe{T}, Func{T})"/>
        public static async ValueTask<T> GetValueOrDefault<T>(this Maybe<T> maybe, Func<ValueTask<T>> valueTask)
        {
            if (maybe.HasNoValue)
                return await valueTask();

            return maybe.GetValueOrThrow();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, K> selector,
            Func<ValueTask<K>> valueTask)
        {
            if (maybe.HasNoValue)
                return await valueTask();

            return selector(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, K)"/>
        public static async ValueTask<K?> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, ValueTask<K>> valueTask,
            K? defaultValue = default)
        {
            if (maybe.HasNoValue)
                return defaultValue;

            return await valueTask(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.GetValueOrDefault{T, K}(in Maybe{T}, Func{T, K}, Func{K})"/>
        public static async ValueTask<K> GetValueOrDefault<T, K>(this Maybe<T> maybe, Func<T, ValueTask<K>> valueTask,
            Func<ValueTask<K>> defaultValue)
        {
            if (maybe.HasNoValue)
                return await defaultValue();

            return await valueTask(maybe.GetValueOrThrow());
        }
    }
}
