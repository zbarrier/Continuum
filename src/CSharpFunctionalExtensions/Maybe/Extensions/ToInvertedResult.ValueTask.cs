using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.ToInvertedResult{T}(in Maybe{T}, Error)"/>
        public static async ValueTask<Result> ToInvertedResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error)
        {
            Maybe<T> maybe = await maybeTask;
            return maybe.ToInvertedResult(error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.ToInvertedResult{T}(in Maybe{T}, Func{Error})"/>
        public static async ValueTask<Result> ToInvertedResult<T>(this ValueTask<Maybe<T>> maybeTask, Func<Error> errorFunc)
        {
            Maybe<T> maybe = await maybeTask;
            return maybe.ToInvertedResult(errorFunc);
        }
    }
}
