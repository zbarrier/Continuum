using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns a list containing the inner value, or an empty list when there is no value.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <returns>A list with zero or one element.</returns>
        public static List<T> ToList<T>(in this Maybe<T> maybe)
        {
            return maybe.HasValue ? new List<T> { maybe.GetValueOrThrow() } : new List<T>();
        }
    }
}
