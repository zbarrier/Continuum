using System;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns the first element of <paramref name="source"/>, or an empty instance when the sequence is empty.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The sequence to search.</param>
        /// <returns>The first element, or an empty instance.</returns>
        public static Maybe<T> TryFirst<T>(this IEnumerable<T> source)
        {
            source = source as ICollection<T> ?? source.ToList();

            if (source.Any()) return Maybe<T>.From(source.First());

            return Maybe<T>.None;
        }

        /// <summary>
        ///     Returns the first element matching <paramref name="predicate"/>, or an empty instance when none match.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The sequence to search.</param>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The first matching element, or an empty instance.</returns>
        public static Maybe<T> TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var firstOrEmpty = source.Where(predicate).Take(1).ToList();
            if (firstOrEmpty.Any()) return Maybe<T>.From(firstOrEmpty[0]);

            return Maybe<T>.None;
        }
    }
}
