using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        public static Result ToInvertedResult<T>(in this Maybe<T> maybe, Error error)
        {
            if (maybe.HasValue)
                return Result.Failure<T>(error);

            return Result.Success();
        }
        
        public static Result ToInvertedResult<T>(in this Maybe<T> maybe, Func<Error> errorFunc)
        {
            if (maybe.HasValue)
                return Result.Failure(errorFunc());
        
            return Result.Success();
        }
    }
}
