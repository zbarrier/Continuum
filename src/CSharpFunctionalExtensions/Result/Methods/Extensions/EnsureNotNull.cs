#nullable enable

using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Returns a new failure result if the result is null. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(this Result<T?> result, Error error)
            where T : class
        {
            return result.Ensure(value => value != null, error).Map(value => value!);
        }

        /// <summary>
        ///     Returns a new failure result if the result is null. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(this Result<T?> result, Error error)
            where T : struct
        {
            return result.Ensure(value => value != null, error).Map(value => value!.Value);
        }

        /// <summary>
        ///     Returns a new failure result if the result is null. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(this Result<T?> result, Func<Error> errorFactory)
            where T : class
        {
            return result.Ensure(value => value != null, errorFactory).Map(value => value!);
        }

        /// <summary>
        ///     Returns a new failure result if the result is null. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> EnsureNotNull<T>(this Result<T?> result, Func<Error> errorFactory)
            where T : struct
        {
            return result.Ensure(value => value != null, errorFactory).Map(value => value!.Value);
        }
    }
}
