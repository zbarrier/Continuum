#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.CSharpFunctionalExtensions
{
    /// <summary>
    ///     Represents an optional value: either a value of type <typeparamref name="T"/> or no value.
    /// </summary>
    /// <typeparam name="T">The type of the optional value.</typeparam>
    [Serializable]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public readonly partial struct Maybe<T> : IEquatable<Maybe<T>>, IEquatable<object>, IMaybe<T>
    {
        private readonly bool _isValueSet;

        private readonly T? _value;

        /// <summary>
        /// Returns the inner value if there's one, otherwise throws an InvalidOperationException with <paramref name="errorMessage"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">Maybe has no value.</exception>
        public T GetValueOrThrow(string? errorMessage = null)
        {
            if (HasNoValue)
                throw new InvalidOperationException(errorMessage ?? Configuration.NoValueException);

            return _value;
        }

        /// <summary>
        /// Returns the inner value if there's one, otherwise throws a custom exception with <paramref name="exception"/>
        /// </summary>
        /// <exception cref="Exception">Maybe has no value.</exception>
        public T GetValueOrThrow(Exception exception)
        {
            if (HasNoValue)
                throw exception;

            return _value;
        }

        /// <summary>
        ///     Returns the inner value if there's one, otherwise <paramref name="defaultValue"/>.
        /// </summary>
        /// <param name="defaultValue">The value returned when there is no inner value.</param>
        public T GetValueOrDefault(T defaultValue)
        {
            if (HasNoValue)
                return defaultValue;

            return _value;
        }

        /// <summary>
        ///     Returns the inner value if there's one, otherwise <see langword="default"/>.
        /// </summary>
        public T? GetValueOrDefault()
        {
            if (HasNoValue)
                return default;

            return _value;
        }

        /// <summary>
        ///  Indicates whether the inner value is present and returns the value if it is.
        /// </summary>
        /// <param name="value">The inner value, if present; otherwise `default`</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(
            [NotNullWhen(true), MaybeNullWhen(false)]
            out T? value)
        {
            value = _value;
            return _isValueSet;
        }

        /// <summary>
        /// Try to use GetValueOrThrow() or GetValueOrDefault() instead for better explicitness.
        /// </summary>
        public T Value => GetValueOrThrow();

        /// <summary>
        ///     Gets an instance with no value.
        /// </summary>
        public static Maybe<T> None => new Maybe<T>();

        /// <summary>
        ///     Gets a value indicating whether an inner value is present.
        /// </summary>
        [MemberNotNullWhen(true, "_value")]
        public bool HasValue => _isValueSet;

        /// <summary>
        ///     Gets a value indicating whether no inner value is present.
        /// </summary>
        [MemberNotNullWhen(false, "_value")]
        public bool HasNoValue => !HasValue;

        private Maybe(T? value)
        {
            if (value == null)
            {
                _isValueSet = false;
                _value = default;
                return;
            }

            _isValueSet = true;
            _value = value;
        }

        /// <summary>
        ///     Wraps <paramref name="value"/>; a <see langword="null"/> value produces <see cref="None"/>.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public static implicit operator Maybe<T>(T? value)
        {
            if (value is Maybe<T> m)
            {
                return m;
            }

            return Maybe.From(value);
        }

        /// <summary>
        ///     Converts the non-generic <see cref="Maybe.None"/> into <see cref="None"/>.
        /// </summary>
        public static implicit operator Maybe<T>(Maybe _) => None;

        /// <summary>
        ///     Creates a new <see cref="Maybe{T}"/> from <paramref name="value"/>; a <see langword="null"/> value produces <see cref="None"/>.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public static Maybe<T> From(T? value)
        {
            return new Maybe<T>(value);
        }
        
        /// <summary>
        ///     Creates a new <see cref="Maybe{T}"/> from the value returned by <paramref name="func"/>.
        /// </summary>
        /// <param name="func">Produces the value to wrap.</param>
        public static Maybe<T> From(Func<T?> func)
        {
            T? value = func();
            
            return new Maybe<T>(value);
        }
        
        /// <summary>
        ///     Awaits <paramref name="valueTask"/> and wraps its result in a <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="valueTask">The task producing the value to wrap.</param>
        public static async Task<Maybe<T>> From(Task<T?> valueTask)
        {
            T? value = await valueTask;
            
            return new Maybe<T>(value);
        }
        
        /// <summary>
        ///     Invokes and awaits <paramref name="valueTaskFunc"/> and wraps its result in a <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="valueTaskFunc">Produces the task whose result is wrapped.</param>
        public static async Task<Maybe<T>> From(Func<Task<T?>> valueTaskFunc)
        {
            T? value = await valueTaskFunc();
            
            return new Maybe<T>(value);
        }

        /// <summary>
        ///     Determines whether <paramref name="maybe"/> contains a value equal to <paramref name="value"/>.
        /// </summary>
        public static bool operator ==(Maybe<T> maybe, T? value)
        {
            if (value is Maybe<T> maybeValue)
                return maybe.Equals(maybeValue);

            if (maybe.HasNoValue)
                return value == null;

            return EqualityComparer<T>.Default.Equals(maybe._value, value);
        }

        /// <summary>
        ///     Determines whether <paramref name="maybe"/> does not contain a value equal to <paramref name="value"/>.
        /// </summary>
        public static bool operator !=(Maybe<T> maybe, T? value)
        {
            return !(maybe == value);
        }

        /// <summary>
        ///     Determines whether <paramref name="maybe"/> equals <paramref name="other"/>.
        /// </summary>
        public static bool operator ==(Maybe<T> maybe, object other)
        {
            return maybe.Equals(other);
        }

        /// <summary>
        ///     Determines whether <paramref name="maybe"/> does not equal <paramref name="other"/>.
        /// </summary>
        public static bool operator !=(Maybe<T> maybe, object other)
        {
            return !(maybe == other);
        }

        /// <summary>
        ///     Determines whether two instances are equal. Two empty instances are equal.
        /// </summary>
        public static bool operator ==(Maybe<T> first, Maybe<T> second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines whether two instances are not equal.
        /// </summary>
        public static bool operator !=(Maybe<T> first, Maybe<T> second)
        {
            return !(first == second);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is Maybe<T> otherMaybe)
                return Equals(otherMaybe);

            if (obj is T otherValue)
                return Equals(otherValue);

            return false;
        }

        /// <summary>
        ///     Determines whether this instance equals <paramref name="other"/>. Two empty instances are equal; otherwise the inner values are compared with the default equality comparer.
        /// </summary>
        /// <param name="other">The instance to compare with.</param>
        public bool Equals(Maybe<T> other)
        {
            if (HasNoValue && other.HasNoValue)
                return true;

            if (HasNoValue || other.HasNoValue)
                return false;

            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (HasNoValue)
                return 0;

            return _value.GetHashCode();
        }

        /// <summary>
        ///     Returns the inner value's string representation, or <c>No value</c> when empty.
        /// </summary>
        public override string ToString()
        {
            if (HasNoValue)
                return "No value";

            return _value.ToString() ?? _value.GetType().Name;
        }
    }

    /// <summary>
    /// Non-generic entrypoint for <see cref="Maybe{T}" /> members
    /// </summary>
    public readonly struct Maybe
    {
        /// <summary>
        ///     Gets an empty value that converts implicitly to <see cref="Maybe{T}.None"/> for any <c>T</c>.
        /// </summary>
        public static Maybe None => new();

        /// <summary>
        /// Creates a new <see cref="Maybe{T}" /> from the provided <paramref name="value"/>
        /// </summary>
        public static Maybe<T> From<T>(T? value) => Maybe<T>.From(value);
        
        /// <summary>
        /// Creates a new <see cref="Maybe{T}" /> from the provided <paramref name="func"/>
        /// </summary>
        public static Maybe<T> From<T>(Func<T?> func) => Maybe<T>.From(func);
        
        /// <summary>
        /// Creates a new <see cref="Maybe{T}" /> from the provided <paramref name="valueTask"/>
        /// </summary>
        public static Task<Maybe<T>> From<T>(Task<T?> valueTask) => Maybe<T>.From(valueTask);
        
        /// <summary>
        /// Creates a new <see cref="Maybe{T}" /> from the provided <paramref name="valueTaskFunc"/>
        /// </summary>
        public static Task<Maybe<T>> From<T>(Func<Task<T?>> valueTaskFunc) => Maybe<T>.From(valueTaskFunc);
    }

    /// <summary>
    /// Useful in scenarios where you need to determine if a value is Maybe or not
    /// </summary>
    public interface IMaybe<out T>
    {
        /// <summary>
        ///     Gets the inner value; throws when there is no value.
        /// </summary>
        T Value { get; }
        /// <summary>
        ///     Gets a value indicating whether an inner value is present.
        /// </summary>
        bool HasValue { get; }
        /// <summary>
        ///     Gets a value indicating whether no inner value is present.
        /// </summary>
        bool HasNoValue { get; }
    }
}
