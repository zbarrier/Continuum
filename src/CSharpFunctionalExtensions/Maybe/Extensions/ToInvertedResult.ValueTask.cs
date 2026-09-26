#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        public static async ValueTask<Result> ToInvertedResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error)
        {
            Maybe<T> maybe = await maybeTask;
            return maybe.ToInvertedResult(error);
        }

        public static async ValueTask<Result> ToInvertedResult<T>(this ValueTask<Maybe<T>> maybeTask, Func<Error> errorFunc)
        {
            Maybe<T> maybe = await maybeTask;
            return maybe.ToInvertedResult(errorFunc);
        }
    }
}
#endif
