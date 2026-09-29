using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K}(in Maybe{T}, Func{T, Maybe{K}})"/>
        public static async ValueTask<Maybe<K>> Bind<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<Maybe<K>>> selector)
        {
            Maybe<T> maybe = await maybeTask;
            return await maybe.Bind(selector);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K, TContext}(in Maybe{T}, Func{T, TContext, Maybe{K}}, TContext)"/>
        public static async ValueTask<Maybe<K>> Bind<T, K, TContext>(
                this ValueTask<Maybe<T>> maybeTask,
                Func<T, TContext, ValueTask<Maybe<K>>> selector,
                TContext context)
        {
            Maybe<T> maybe = await maybeTask;
            return await maybe.Bind(selector, context);
        }
    }
}
