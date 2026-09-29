namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <summary>
        ///     Returns a success with the inner value, or a failure with <paramref name="error"/> when empty.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="error">The error used when empty.</param>
        /// <returns>A success with the value, or a failure.</returns>
        public static Result<T> ToResult<T>(in this Maybe<T> maybe, Error error)
        {
            if (maybe.HasNoValue)
                return Result.Failure<T>(error);

            return Result.Success(maybe.GetValueOrThrow());
        }

        /// <summary>
        ///     Returns a success with the inner value, or a failure with the error produced by <paramref name="errorFunc"/> when empty.
        /// </summary>
        /// <typeparam name="T">The type of the inner value.</typeparam>
        /// <param name="maybe">The source instance.</param>
        /// <param name="errorFunc">Produces the error; only invoked when empty.</param>
        /// <returns>A success with the value, or a failure.</returns>
        public static Result<T> ToResult<T>(in this Maybe<T> maybe, Func<Error> errorFunc)
        {
            if (maybe.HasNoValue)
                return Result.Failure<T>(errorFunc());

            return Result.Success<T>(maybe.GetValueOrThrow());
        }
    }
}
