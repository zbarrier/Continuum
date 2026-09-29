using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns the value stored under <paramref name="key"/>, or an empty instance when the key is not present.
        /// </summary>
        /// <typeparam name="K">The key type.</typeparam>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="dict">The dictionary to search.</param>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value, or an empty instance.</returns>
        public static Maybe<V> TryFind<K, V>(this IReadOnlyDictionary<K, V> dict, K key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return Maybe<V>.None;
        }
    }
}
