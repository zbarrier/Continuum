using System;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    /// Use non-generic ValueObject whenever possible: http://bit.ly/vo-new
    /// </summary>
    [Serializable]
    public abstract class ValueObject<T>
        where T : ValueObject<T>
    {
        private int? _cachedHashCode;

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            var valueObject = obj as T;

            if (valueObject is null)
                return false;

            if (ValueObject.GetUnproxiedType(this) != ValueObject.GetUnproxiedType(valueObject))
                return false;

            return EqualsCore(valueObject);
        }

        /// <summary>
        ///     Determines whether this instance equals <paramref name="other"/>, which is guaranteed to be non-null and of the same type.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        protected abstract bool EqualsCore(T other);

        /// <inheritdoc/>
        /// <remarks>
        ///     The hash code is computed once via <see cref="GetHashCodeCore"/> and cached.
        /// </remarks>
        public override int GetHashCode()
        {
            if (!_cachedHashCode.HasValue)
            {
                _cachedHashCode = GetHashCodeCore();
            }

            return _cachedHashCode.Value;
        }

        /// <summary>
        ///     Computes the hash code for this instance; must be consistent with <see cref="EqualsCore(T)"/>.
        /// </summary>
        protected abstract int GetHashCodeCore();

        /// <summary>
        ///     Determines whether two value objects are equal. Two <see langword="null"/> references are equal.
        /// </summary>
        public static bool operator ==(ValueObject<T> a, ValueObject<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        /// <summary>
        ///     Determines whether two value objects are not equal.
        /// </summary>
        public static bool operator !=(ValueObject<T> a, ValueObject<T> b)
        {
            return !(a == b);
        }
    }
}
