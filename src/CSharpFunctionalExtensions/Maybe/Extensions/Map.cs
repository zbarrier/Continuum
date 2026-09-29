using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Transforms the inner value with <paramref name="selector"/>; returns <see cref="Maybe{T}.None"/> when there is no value.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the resulting value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Transforms the inner value.</param>
        /// <returns>The transformed instance, or an empty instance.</returns>
        public static Maybe<K> Map<T, K>(in this Maybe<T> maybe, Func<T, K> selector)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return selector(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static Maybe<K> Map<T, K, TContext>(
            in this Maybe<T> maybe,
            Func<T, TContext, K> selector,
            TContext context
        )
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return selector(maybe.GetValueOrThrow(), context);
        }
    }
}
