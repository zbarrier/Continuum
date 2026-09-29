using System;
using System.Collections.Generic;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Base class for value objects that wrap a single comparable value.
    /// </summary>
    /// <typeparam name="T">The type of the wrapped value.</typeparam>
    [Serializable]
    public abstract class SimpleValueObject<T> : ComparableValueObject
        where T : IComparable
    {
        /// <summary>
        ///     Gets the wrapped value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        ///     Initializes a new instance wrapping <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        protected SimpleValueObject(T value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        protected override IEnumerable<IComparable> GetComparableEqualityComponents()
        {
            yield return Value;
        }

        /// <summary>
        ///     Returns the string representation of <see cref="Value"/>.
        /// </summary>
        public override string? ToString()
        {
            return Value?.ToString();
        }

        /// <summary>
        ///     Unwraps the value; a <see langword="null"/> instance produces <see langword="default"/>.
        /// </summary>
        public static implicit operator T?(SimpleValueObject<T>? valueObject)
        {
            return valueObject is null ? default : valueObject.Value;
        }
    }
}
