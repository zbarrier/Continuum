using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns the inner value, or the value produced by <paramref name="defaultValue"/> when empty.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="defaultValue">Produces the fallback value; only invoked when empty.</param>
        /// <returns>The inner value or the fallback.</returns>
        public static T GetValueOrDefault<T>(in this Maybe<T> maybe, Func<T> defaultValue)
        {
            if (maybe.HasNoValue)
                return defaultValue();

            return maybe.GetValueOrThrow();
        }

        /// <summary>
        ///     Projects the inner value with <paramref name="selector"/>, or returns <paramref name="defaultValue"/> when empty.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <typeparam name="K">The type of the projected value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Projects the inner value.</param>
        /// <param name="defaultValue">The fallback value.</param>
        /// <returns>The projected value or the fallback.</returns>
        public static K? GetValueOrDefault<T, K>(in this Maybe<T> maybe, Func<T, K> selector, K? defaultValue = default)
        {
            if (maybe.HasNoValue)
                return defaultValue;

            return selector(maybe.GetValueOrThrow());
        }

        /// <summary>
        ///     Projects the inner value with <paramref name="selector"/>, or returns the value produced by <paramref name="defaultValue"/> when empty.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <typeparam name="K">The type of the projected value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="selector">Projects the inner value.</param>
        /// <param name="defaultValue">Produces the fallback value; only invoked when empty.</param>
        /// <returns>The projected value or the fallback.</returns>
        public static K GetValueOrDefault<T, K>(in this Maybe<T> maybe, Func<T, K> selector, Func<K> defaultValue)
        {
            if (maybe.HasNoValue)
                return defaultValue();

            return selector(maybe.GetValueOrThrow());
        }
    }
}
