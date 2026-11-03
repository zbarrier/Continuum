using System;
using System.Collections;
using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Base class for value objects that support ordering by comparing their components in sequence.
    /// </summary>
    [Serializable]
    public abstract class ComparableValueObject : ValueObject, IComparable, IComparable<ComparableValueObject>
    {
        /// <summary>
        ///     Returns the comparable components used for equality, hashing, and ordering, in a stable order.
        /// </summary>
        protected abstract IEnumerable<IComparable> GetComparableEqualityComponents();

        /// <inheritdoc/>
        protected sealed override IEnumerable<object> GetEqualityComponents() => GetComparableEqualityComponents();

        /// <summary>
        ///     Compares this instance with <paramref name="other"/> component by component. Instances of different types are ordered by type name.
        /// </summary>
        /// <param name="other">The instance to compare with; <see langword="null"/> sorts first.</param>
        /// <returns>A negative value, zero, or a positive value indicating relative order.</returns>
        public virtual int CompareTo(ComparableValueObject? other)
        {
            if (other is null)
                return 1;

            if (ReferenceEquals(this, other))
                return 0;

            Type thisType = GetUnproxiedType(this);
            Type otherType = GetUnproxiedType(other);
            if (thisType != otherType)
                return string.CompareOrdinal(thisType.ToString(), otherType.ToString());

            using IEnumerator<IComparable> left = GetComparableEqualityComponents().GetEnumerator();
            using IEnumerator<IComparable> right = other.GetComparableEqualityComponents().GetEnumerator();
            while (left.MoveNext() && right.MoveNext())
            {
                int cmp = left.Current?.CompareTo(right.Current) ?? (right.Current is null ? 0 : -1);
                if (cmp != 0)
                    return cmp;
            }

            return 0;
        }

        /// <inheritdoc cref="CompareTo(ComparableValueObject)"/>
        public virtual int CompareTo(object? other) 
        {
            if (other is null)
                return 1;

            if (other is not ComparableValueObject comparable)
                throw new ArgumentException($"Object must be of type {nameof(ComparableValueObject)}.", nameof(other));

            return CompareTo(comparable);
        }
    }
}
