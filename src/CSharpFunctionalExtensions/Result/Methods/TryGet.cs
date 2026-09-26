using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.CSharpFunctionalExtensions
{
    partial struct Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error)
        {
            error = _error;
            return IsFailure;
        }
    }

    partial struct Result<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue([NotNullWhen(true), MaybeNullWhen(false)] out T value)
        {
            value = _value;
            return IsSuccess;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error)
        {
            error = _error;
            return IsFailure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue([NotNullWhen(true), MaybeNullWhen(false)] out T value, [NotNullWhen(false), MaybeNullWhen(true)] out Error error)
        {
            value = _value;
            error = _error;
            return IsSuccess;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetError([NotNullWhen(true), MaybeNullWhen(false)] out Error error, [NotNullWhen(false), MaybeNullWhen(true)] out T value)
        {
            value = _value;
            error = _error;
            return IsFailure;
        }
    }
}
