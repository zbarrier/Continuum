using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T, Task<bool>> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(errorPredicate(result.Value));

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T, Task<bool>> predicate, Func<T, Task<Error>> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(await errorPredicate(result.Value).DefaultAwait());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Result result, Func<Task<bool>> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate().DefaultAwait())
                return Result.Failure(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Result result, Func<Task<bool>> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate().DefaultAwait())
                return Result.Failure(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Result result, Func<Task<Result>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate().DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<Task<Result>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate().DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure<T>(this Result result, Func<Task<Result<T>>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate().DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<Task<Result<T>>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate().DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T,Task<Result>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate(result.Value).DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Result<T> result, Func<T,Task<Result<T>>> predicate)
        {
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate(result.Value).DefaultAwait();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
    }
}
