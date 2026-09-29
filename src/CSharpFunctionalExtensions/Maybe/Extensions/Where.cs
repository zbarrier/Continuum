using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns the instance when its value satisfies <paramref name="predicate"/>; otherwise an empty instance.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="predicate">The condition the value must satisfy.</param>
        /// <returns>The original instance or an empty instance.</returns>
        public static Maybe<T> Where<T>(in this Maybe<T> maybe, Func<T, bool> predicate)
        {
            if (maybe.HasNoValue)
                return Maybe<T>.None;

            if (predicate(maybe.GetValueOrThrow()))
                return maybe;

            return Maybe<T>.None;
        }
    }
}
