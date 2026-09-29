using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static async ValueTask<Maybe<K>> Map<T, K>(this ValueTask<Maybe<T>> valueTask, Func<T, K> selector)
        {
            Maybe<T> maybe = await valueTask;
            return maybe.Map(selector);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K, TContext}(in Maybe{T}, Func{T, TContext, K}, TContext)"/>
        public static async ValueTask<Maybe<K>> Map<T, K, TContext>(
            this ValueTask<Maybe<T>> valueTask,
            Func<T, TContext, K> selector,
            TContext context
        )
        {
            Maybe<T> maybe = await valueTask;
            return maybe.Map(selector, context);
        }
    }
}
