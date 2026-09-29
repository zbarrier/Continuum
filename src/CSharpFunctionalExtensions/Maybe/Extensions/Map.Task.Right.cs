using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static async Task<Maybe<K>> Map<T, K>(this Maybe<T> maybe, Func<T, Task<K>> selector)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return await selector(maybe.GetValueOrThrow()).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K, TContext}(in Maybe{T}, Func{T, TContext, K}, TContext)"/>
        public static async Task<Maybe<K>> Map<T, K, TContext>(
            this Maybe<T> maybe,
            Func<T, TContext, Task<K>> selector,
            TContext context
        )
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return await selector(maybe.GetValueOrThrow(), context).DefaultAwait();
        }
    }
}
