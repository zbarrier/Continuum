using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.CSharpFunctionalExtensions
{
    partial struct Result
    {
        /// <summary>
        ///     Gets the error when the result is a failure.
        /// </summary>
        /// <param name="error">The error when the result is a failure; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the result is a failure; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error)
        {
            error = _error;
            return IsFailure;
        }
    }

    partial struct Result<T>
    {
        /// <summary>
        ///     Gets the value when the result is a success.
        /// </summary>
        /// <param name="value">The value when the result is a success; otherwise <see langword="default"/>.</param>
        /// <returns><see langword="true"/> if the result is a success; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue([NotNullWhen(true), MaybeNullWhen(false)] out T value)
        {
            value = _value;
            return IsSuccess;
        }

        /// <inheritdoc cref="Result.TryGetError(out Error)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error)
        {
            error = _error;
            return IsFailure;
        }

        /// <inheritdoc cref="TryGetValue(out T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue([NotNullWhen(true), MaybeNullWhen(false)] out T value, [NotNullWhen(false), MaybeNullWhen(true)] out Error error)
        {
            value = _value;
            error = _error;
            return IsSuccess;
        }

        /// <inheritdoc cref="TryGetError(out Error)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error, [NotNullWhen(false), MaybeNullWhen(true)] out T value)
        {
            value = _value;
            error = _error;
            return IsFailure;
        }
    }
}
