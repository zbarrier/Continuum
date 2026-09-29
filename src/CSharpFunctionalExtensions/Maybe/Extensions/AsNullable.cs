using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        // ReSharper disable ConvertNullableToShortForm
        /// <summary>
        /// Converts the <see cref="Maybe{T}"/> to a <see cref="Nullable"/> struct.
        /// </summary>
        /// <returns>Returns the <see cref="Nullable{T}"/> equivalent to the <see cref="Maybe{T}"/>.</returns>
        public static Nullable<T> AsNullable<T>(ref this Maybe<T> value)
            where T : struct
        {
            return System.Runtime.CompilerServices.Unsafe.As<Maybe<T>, Nullable<T>>(ref value);
        }
        // ReSharper restore ConvertNullableToShortForm
    }
}