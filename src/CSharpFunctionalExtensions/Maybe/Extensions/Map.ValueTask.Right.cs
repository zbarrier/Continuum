using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K}(in Maybe{T}, Func{T, K})"/>
        public static async ValueTask<Maybe<K>> Map<T, K>(this Maybe<T> maybe, Func<T, ValueTask<K>> valueTask)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return await valueTask(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Map{T, K, TContext}(in Maybe{T}, Func{T, TContext, K}, TContext)"/>
        public static async ValueTask<Maybe<K>> Map<T, K, TContext>(
            this Maybe<T> maybe,
            Func<T, TContext, ValueTask<K>> valueTask,
            TContext context
        )
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None;

            return await valueTask(maybe.GetValueOrThrow(), context);
        }
    }
}
