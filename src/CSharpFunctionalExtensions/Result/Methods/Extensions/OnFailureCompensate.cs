using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Result<T> OnFailureCompensate<T>(this Result<T> result, Func<Result<T>> func)
        {
            if (result.IsFailure)
                return func();

            return result;
        }
        
        public static Result OnFailureCompensate(this Result result, Func<Result> func)
        {
            if (result.IsFailure)
                return func();

            return result;
        }
        
        public static Result<T> OnFailureCompensate<T>(this Result<T> result, Func<Error, Result<T>> func)
        {
            if (result.IsFailure)
                return func(result.Error);

            return result;
        }

        public static Result OnFailureCompensate(this Result result, Func<Error, Result> func)
        {
            if (result.IsFailure)
                return func(result.Error);

            return result;
        }
    }
}
