using System.Threading.Tasks;
using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
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
        public static async Task<Result<K>> BindTry<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure<K>(result.Error)
                : await Result.Try(() => func(result.Value), errorHandler).Bind(r => r).DefaultAwait();
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
        public static async Task<Result<K>> BindTry<K>(this Result result, Func<Task<Result<K>>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure<K>(result.Error)
                : await Result.Try(() => func(), errorHandler).Bind(r => r).DefaultAwait();
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>
        /// <typeparam name="T">Result Type parameter</typeparam>        
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static async Task<Result> BindTry<T>(this Result<T> result, Func<T, Task<Result>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : await Result.Try(() => func(result.Value), errorHandler).Bind(r => r).DefaultAwait();
        }

        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a failure, a new failure result is returned instead.
        ///     If a given function throws an exception, an error is returned from the given error handler
        /// </summary>        
        /// <param name="result">Extended result</param>
        /// <param name="func">Function returning result to bind</param>
        /// <param name="errorHandler">Error handling function</param>        
        /// <returns>Binding result</returns>
        public static async Task<Result> BindTry(this Result result, Func<Task<Result>> func,
            Func<Exception, Error> errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : await Result.Try(() => func(), errorHandler).Bind(r => r).DefaultAwait();
        }
    }
}
