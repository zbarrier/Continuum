using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>
        /// <typeparam name="T">Result Type parameter</typeparam>        
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static Result BindTry<T>(this Result<T> result, Func<T, Result> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : Result.Try(() => func(result.Value), errorHandler).Bind(r => r);
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>
        /// <typeparam name="T">Result Type parameter</typeparam>
        /// <typeparam name="K"><paramref name="func" /> Result Type parameter</typeparam>        
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static Result<K> BindTry<T, K>(this Result<T> result, Func<T, Result<K>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure<K>(result.Error)
                : Result.Try(() => func(result.Value), errorHandler).Bind(r => r);
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>        
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static Result BindTry(this Result result, Func<Result> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : Result.Try(() => func(), errorHandler).Bind(r => r);
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>        
        /// <typeparam name="K"><paramref name="func" /> Result Type parameter</typeparam>
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static Result<K> BindTry<K>(this Result result, Func<Result<K>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure<K>(result.Error)
                : Result.Try(() => func(), errorHandler).Bind(r => r);
        }
    }
}