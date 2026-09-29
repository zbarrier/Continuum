using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K}(in Maybe{T}, Func{T, Maybe{K}})"/>
        public static async Task<Maybe<K>> Bind<T, K>(this Task<Maybe<T>> maybeTask, Func<T, Maybe<K>> selector)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.Bind(selector);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K, TContext}(in Maybe{T}, Func{T, TContext, Maybe{K}}, TContext)"/>
        public static async Task<Maybe<K>> Bind<T, K, TContext>(this Task<Maybe<T>> maybeTask, Func<T, TContext, Maybe<K>> selector, TContext context)
        {
            var maybe = await maybeTask.DefaultAwait();
            return maybe.Bind(selector, context);
        }
    }
}
