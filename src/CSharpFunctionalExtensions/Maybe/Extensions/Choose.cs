using System;
using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Projects the inner values of the instances that have a value, skipping empty ones.
        /// </summary>
        /// <typeparam name="T">The type of the inner values.</typeparam>
        /// <typeparam name="U">The type of the projected values.</typeparam>
        /// <param name="source">The instances to filter.</param>
        /// <param name="selector">Projects each inner value.</param>
        /// <returns>The projected values.</returns>
        public static IEnumerable<U> Choose<T, U>(this IEnumerable<Maybe<T>> source, Func<T, U> selector)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (item.HasValue) yield return selector(item.GetValueOrThrow());
                }
            }
        }

        /// <summary>
        ///     Returns the inner values of the instances that have a value, skipping empty ones.
        /// </summary>
        /// <typeparam name="T">The type of the inner values.</typeparam>
        /// <param name="source">The instances to filter.</param>
        /// <returns>The inner values.</returns>
        public static IEnumerable<T> Choose<T>(this IEnumerable<Maybe<T>> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    if (item.HasValue) yield return item.GetValueOrThrow();
                }
            }
        }
    }
}
