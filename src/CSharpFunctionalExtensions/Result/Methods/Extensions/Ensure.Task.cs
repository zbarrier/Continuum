using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Error error)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<Error> errorPredicate)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, Error> errorPredicate)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(errorPredicate(result.Value));

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, Func<T, Task<Error>> errorPredicate)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value).DefaultAwait())
                return Result.Failure<T>(await errorPredicate(result.Value).DefaultAwait());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Task<Result> resultTask, Func<Task<bool>> predicate, Error error)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate().DefaultAwait())
                return Result.Failure(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Task<Result> resultTask, Func<Task<bool>> predicate, Func<Error> errorPredicate)
        {
            Result result = await resultTask.DefaultAwait();

            if (result.IsFailure)
                return result;

            if (!await predicate().DefaultAwait())
                return Result.Failure(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure(this Task<Result> resultTask, Func<Task<Result>> predicate)
        {
          Result result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate();
          
          if (predicateResult.IsFailure)
            return Result.Failure(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<Task<Result>> predicate)
        {
          Result<T> result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result> Ensure<T>(this Task<Result> resultTask, Func<Task<Result<T>>> predicate)
        {
          Result result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> predicate)
        {
          Result<T> result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;
        
          var predicateResult = await predicate();
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);
        
          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T,Task<Result>> predicate)
        {
          Result<T> result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate(result.Value);
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async Task<Result<T>> Ensure<T>(this Task<Result<T>> resultTask, Func<T,Task<Result<T>>> predicate)
        {
          Result<T> result = await resultTask.DefaultAwait();
          
          if (result.IsFailure)
            return result;

          var predicateResult = await predicate(result.Value);
          
          if (predicateResult.IsFailure)
            return Result.Failure<T>(predicateResult.Error);

          return result;
        }
    }
}
