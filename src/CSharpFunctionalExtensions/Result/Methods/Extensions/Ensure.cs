using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Failure<T>(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Failure<T>(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Func<T, Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!predicate(result.Value))
                return Result.Failure<T>(errorPredicate(result.Value));

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static Result Ensure(this Result result, Func<bool> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!predicate())
                return Result.Failure(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static Result Ensure(this Result result, Func<bool> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!predicate())
                return Result.Failure(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result Ensure(this Result result, Func<Result> predicate)
        {
            if (result.IsFailure)
                return result;

            var predicateResult = predicate();
          
            if (predicateResult.IsFailure)
                return Result.Failure(predicateResult.Error);

            return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<Result> predicate)
        {
            if (result.IsFailure)
                return result;
        
            var predicateResult = predicate();
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);
        
            return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result Ensure<T>(this Result result, Func<Result<T>> predicate)
        {
            if (result.IsFailure)
                return result;
        
            var predicateResult = predicate();
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);
        
            return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<Result<T>> predicate)
        {
            if (result.IsFailure)
                return result;

            var predicateResult = predicate();
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<T,Result> predicate)
        {
            if (result.IsFailure)
                return result;
        
            var predicateResult = predicate(result.Value);
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);
        
            return result;
        }
        
        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static Result<T> Ensure<T>(this Result<T> result, Func<T,Result<T>> predicate)
        {
            if (result.IsFailure)
                return result;
        
            var predicateResult = predicate(result.Value);
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);
        
            return result;
        }
    }
}
