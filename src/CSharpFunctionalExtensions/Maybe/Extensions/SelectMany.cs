using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     LINQ query-syntax alias for <see cref="Bind{T, K}(in Maybe{T}, Func{T, Maybe{K}})"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the resulting value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Produces the next optional value.</param>
        /// <returns>The result of <paramref name="selector"/>, or an empty instance.</returns>
        public static Maybe<K> SelectMany<T, K>(in this Maybe<T> maybe, Func<T, Maybe<K>> selector)
        {
            return maybe.Bind(selector);
        }

        /// <summary>
        ///     Supports LINQ query syntax with multiple <c>from</c> clauses: binds with <paramref name="selector"/> and projects both values with <paramref name="project"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="U">The type of the intermediate value.</typeparam>
        /// <typeparam name="V">The type of the projected value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Produces the intermediate optional value.</param>
        /// <param name="project">Combines the source and intermediate values.</param>
        /// <returns>The projected instance, or an empty instance.</returns>
        public static Maybe<V> SelectMany<T, U, V>(in this Maybe<T> maybe,
            Func<T, Maybe<U>> selector,
            Func<T, U, V> project)
        {
            return maybe.GetValueOrDefault(
                x => selector(x).GetValueOrDefault(u => project(x, u), Maybe<V>.None),
                Maybe<V>.None);
        }
    }
}
