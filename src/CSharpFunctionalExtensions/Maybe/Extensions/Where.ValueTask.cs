using System;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Where{T}(in Maybe{T}, Func{T, bool})"/>
        public static async ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<bool>> predicate)
        {
            Maybe<T> maybe = await maybeTask.DefaultAwait();
            return await maybe.Where(predicate).DefaultAwait();
        }
    }
}
