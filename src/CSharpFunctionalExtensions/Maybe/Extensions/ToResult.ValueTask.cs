using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.ToResult{T}(in Maybe{T}, Error)"/>
        public static async ValueTask<Result<T>> ToResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error)
        {
            Maybe<T> maybe = await maybeTask.DefaultAwait();
            return maybe.ToResult(error);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.ToResult{T}(in Maybe{T}, Func{Error})"/>
        public static async ValueTask<Result<T>> ToResult<T>(this ValueTask<Maybe<T>> maybeTask, Func<Error> errorFunc)
        {
            Maybe<T> maybe = await maybeTask.DefaultAwait();
            return maybe.ToResult(errorFunc);
        }
    }
}
