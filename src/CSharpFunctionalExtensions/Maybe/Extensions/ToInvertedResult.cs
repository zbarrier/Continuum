using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns a success when empty, or a failure with <paramref name="error"/> when a value is present.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="error">The error used when a value is present.</param>
        /// <returns>A success when empty; otherwise a failure.</returns>
        public static Result ToInvertedResult<T>(in this Maybe<T> maybe, Error error)
        {
            if (maybe.HasValue)
                return Result.Failure<T>(error);

            return Result.Success();
        }
        
        /// <summary>
        ///     Returns a success when empty, or a failure with the error produced by <paramref name="errorFunc"/> when a value is present.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="errorFunc">Produces the error; only invoked when a value is present.</param>
        /// <returns>A success when empty; otherwise a failure.</returns>
        public static Result ToInvertedResult<T>(in this Maybe<T> maybe, Func<Error> errorFunc)
        {
            if (maybe.HasValue)
                return Result.Failure(errorFunc());
        
            return Result.Success();
        }
    }
}
