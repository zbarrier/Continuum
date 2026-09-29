using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static async Task<Maybe<K>> Map<T, K>(this Task<Maybe<T>> maybeTask, Func<T, Task<K>> selector)
        {
            var maybe = await maybeTask.DefaultAwait();
            return await maybe.Map(selector).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K, TContext}(in Maybe{T}, Func{T, TContext, K}, TContext)"/>
        public static async Task<Maybe<K>> Map<T, K, TContext>(
            this Task<Maybe<T>> maybeTask,
            Func<T, TContext, Task<K>> selector,
            TContext context
        )
        {
            var maybe = await maybeTask.DefaultAwait();
            return await maybe.Map(selector, context).DefaultAwait();
        }
    }
}
