using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K}(in Maybe{T}, Func{T, Maybe{K}})"/>
        public static Task<Maybe<K>> Bind<T, K>(this Maybe<T> maybe, Func<T, Task<Maybe<K>>> selector)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None.AsCompletedTask();

            return selector(maybe.GetValueOrThrow());
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Bind{T, K, TContext}(in Maybe{T}, Func{T, TContext, Maybe{K}}, TContext)"/>
        public static Task<Maybe<K>> Bind<T, K, TContext>(
                this Maybe<T> maybe,
                Func<T, TContext, Task<Maybe<K>>> selector,
                TContext context)
        {
            if (maybe.HasNoValue)
                return Maybe<K>.None.AsCompletedTask();

            return selector(maybe.GetValueOrThrow(), context);
        }
    }
}
