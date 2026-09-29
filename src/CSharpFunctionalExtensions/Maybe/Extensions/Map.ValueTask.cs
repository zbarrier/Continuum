using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static async ValueTask<Maybe<K>> Map<T, K>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask<K>> valueTask)
        {
            Maybe<T> maybe = await maybeTask;
            return await maybe.Map(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K, TContext}(in Maybe{T}, Func{T, TContext, K}, TContext)"/>
        public static async ValueTask<Maybe<K>> Map<T, K, TContext>(
            this ValueTask<Maybe<T>> maybeTask,
            Func<T, TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            Maybe<T> maybe = await maybeTask;
            return await maybe.Map(valueTask, context);
        }
    }
}
