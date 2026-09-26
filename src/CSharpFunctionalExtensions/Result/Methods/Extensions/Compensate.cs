using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     If the given result is a success returns a new success result. Otherwise it returns the result of the given function.
        /// </summary>
        public static Result Compensate(this Result result, Func<Error, Result> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            return func(result.Error);
        }

        /// <summary>
        ///     If the given result is a success returns a new success result. Otherwise it returns the result of the given function.
        /// </summary>
        public static Result Compensate<T>(this Result<T> result, Func<Error, Result> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success();
            }

            return func(result.Error);
        }

        /// <summary>
        ///     If the given result is a success returns a new success result. Otherwise it returns the result of the given function.
        /// </summary>
        public static Result<T> Compensate<T>(this Result<T> result, Func<Error, Result<T>> func)
        {
            if (result.IsSuccess)
            {
                return Result.Success(result.Value);
            }

            return func(result.Error);
        }
    }
}