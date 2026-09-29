using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Applies <paramref name="selector"/> to the inner value and returns its result; returns <see cref="Maybe{T}.None"/> when there is no value.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the resulting value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Produces the next optional value.</param>
        /// <returns>The result of <paramref name="selector"/>, or an empty instance.</returns>
        public static Maybe<K> Bind<T, K>(in this Maybe<T> maybe, Func<T, Maybe<K>> selector)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return selector(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Bind{T, K}(in Maybe{T}, Func{T, Maybe{K}})"/>
        public static Maybe<K> Bind<T, K, TContext>(
            in this Maybe<T> maybe,
            Func<T, TContext, Maybe<K>> selector,
            TContext context
        )
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return selector(maybe.GetValueOrThrow(), context);
        }
    }
}
