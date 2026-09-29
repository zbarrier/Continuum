using System;
using System.ComponentModel;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     LINQ query-syntax alias for <see cref="Map{T, K}(in Maybe{T}, Func{T, K})"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the resulting value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Transforms the inner value.</param>
        /// <returns>The transformed instance, or an empty instance.</returns>
        public static Maybe<K> Select<T, K>(in this Maybe<T> maybe, Func<T, K> selector)
        {
            return maybe.Map(selector);
        }
    }
}
