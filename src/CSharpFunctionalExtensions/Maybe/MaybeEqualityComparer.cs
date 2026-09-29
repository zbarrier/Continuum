using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Compares <see cref="Maybe{T}"/> instances using a configurable comparer for the inner values.
    /// </summary>
    /// <typeparam name="T">The type of the inner value.</typeparam>
    public class MaybeEqualityComparer<T> : IEqualityComparer<Maybe<T>>
    {
        private readonly IEqualityComparer<T> _equalityComparer;

        /// <summary>
        ///     Initializes a new comparer.
        /// </summary>
        /// <param name="equalityComparer">The comparer for inner values; defaults to <see cref="EqualityComparer{T}.Default"/>.</param>
        public MaybeEqualityComparer(IEqualityComparer<T>? equalityComparer = null)
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        /// <inheritdoc/>
        public bool Equals(Maybe<T> x, Maybe<T> y)
        {
            if (x.HasNoValue && y.HasNoValue)
            {
                return true;
            }

            return x.HasValue && y.HasValue && _equalityComparer.Equals(x.Value, y.Value);
        }

        /// <inheritdoc/>
        public int GetHashCode(Maybe<T> obj)
        {
            return obj.HasNoValue ? 0 : _equalityComparer.GetHashCode(obj.Value!);
        }
    }
}
