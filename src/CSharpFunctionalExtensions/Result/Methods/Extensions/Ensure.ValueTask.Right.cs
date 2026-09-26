#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<bool>> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value))
                return Result.Failure<T>(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<bool>> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value))
                return Result.Failure<T>(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<bool>> predicate, Func<T, Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value))
                return Result.Failure<T>(errorPredicate(result.Value));

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<bool>> predicate, Func<T, ValueTask<Error>> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate(result.Value))
                return Result.Failure<T>(await errorPredicate(result.Value));

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result> Ensure(this Result result, Func<ValueTask<bool>> predicate, Error error)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate())
                return Result.Failure(error);

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is false. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result> Ensure(this Result result, Func<ValueTask<bool>> predicate, Func<Error> errorPredicate)
        {
            if (result.IsFailure)
                return result;

            if (!await predicate())
                return Result.Failure(errorPredicate());

            return result;
        }

        /// <summary>
        ///     Returns a new failure result if the predicate is a failure result. Otherwise returns the starting result.
        /// </summary>
        public static async ValueTask<Result> Ensure(this Result result, Func<ValueTask<Result>> predicate)
        {
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
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<ValueTask<Result>> predicate)
        {
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
        public static async ValueTask<Result> Ensure<T>(this Result result, Func<ValueTask<Result<T>>> predicate)
        {
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
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<ValueTask<Result<T>>> predicate)
        {
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
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<Result>> predicate)
        {
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
        public static async ValueTask<Result<T>> Ensure<T>(this Result<T> result, Func<T, ValueTask<Result<T>>> predicate)
        {
            if (result.IsFailure)
                return result;

            var predicateResult = await predicate(result.Value);
          
            if (predicateResult.IsFailure)
                return Result.Failure<T>(predicateResult.Error);

            return result;
        }
    }
}
#endif