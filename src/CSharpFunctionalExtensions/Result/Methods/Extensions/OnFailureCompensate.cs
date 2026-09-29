using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     When <paramref name="result"/> is a failure, replaces it with the result of <paramref name="func"/>; otherwise returns it unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="func">Produces the compensating result.</param>
        /// <returns><paramref name="result"/> on success; otherwise the compensating result.</returns>
        public static Result<T> OnFailureCompensate<T>(this Result<T> result, Func<Result<T>> func)
        {
            if (result.IsFailure)
                return func();

            return result;
        }
        
        /// <summary>
        ///     When <paramref name="result"/> is a failure, replaces it with the result of <paramref name="func"/>; otherwise returns it unchanged.
        /// </summary>
        /// <param name="result">The source result.</param>
        /// <param name="func">Produces the compensating result.</param>
        /// <returns><paramref name="result"/> on success; otherwise the compensating result.</returns>
        public static Result OnFailureCompensate(this Result result, Func<Result> func)
        {
            if (result.IsFailure)
                return func();

            return result;
        }
        
        /// <inheritdoc cref="OnFailureCompensate{T}(Result{T}, Func{Result{T}})"/>
        public static Result<T> OnFailureCompensate<T>(this Result<T> result, Func<Error, Result<T>> func)
        {
            if (result.IsFailure)
                return func(result.Error);

            return result;
        }

        /// <inheritdoc cref="OnFailureCompensate(Result, Func{Result})"/>
        public static Result OnFailureCompensate(this Result result, Func<Error, Result> func)
        {
            if (result.IsFailure)
                return func(result.Error);

            return result;
        }
    }
}
