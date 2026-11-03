using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Continuum.CSharpFunctionalExtensions.ValueTasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class MaybeExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TE, T}(in Maybe{T}, Func{T, TE}, Func{TE})"/>
        public static async ValueTask<TE> Match<TE, T>(
            this Maybe<T> maybe,
            Func<T, CancellationToken, ValueTask<TE>> Some,
            Func<CancellationToken, ValueTask<TE>> None,
            CancellationToken cancellationToken = default
        )
        {
            return maybe.HasValue
                ? await Some(maybe.GetValueOrThrow(), cancellationToken).DefaultAwait()
                : await None(cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TE, T, TContext}(in Maybe{T}, Func{T, TContext, TE}, Func{TContext, TE}, TContext)"/>
        public static async ValueTask<TE> Match<TE, T, TContext>(
            this Maybe<T> maybe,
            Func<T, TContext, CancellationToken, ValueTask<TE>> Some,
            Func<TContext, CancellationToken, ValueTask<TE>> None,
            TContext context,
            CancellationToken cancellationToken = default
        )
        {
            return maybe.HasValue
                ? await Some(maybe.GetValueOrThrow(), context, cancellationToken).DefaultAwait()
                : await None(context, cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{T}(in Maybe{T}, Action{T}, Action)"/>
        public static async ValueTask Match<T>(
            this Maybe<T> maybe,
            Func<T, CancellationToken, ValueTask> Some,
            Func<CancellationToken, ValueTask> None,
            CancellationToken cancellationToken = default
        )
        {
            if (maybe.HasValue)
                await Some(maybe.GetValueOrThrow(), cancellationToken).DefaultAwait();
            else
                await None(cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{T, TContext}(in Maybe{T}, Action{T, TContext}, Action{TContext}, TContext)"/>
        public static async ValueTask Match<T, TContext>(
            this Maybe<T> maybe,
            Func<T, TContext, CancellationToken, ValueTask> Some,
            Func<TContext, CancellationToken, ValueTask> None,
            TContext context,
            CancellationToken cancellationToken = default
        )
        {
            if (maybe.HasValue)
                await Some(maybe.GetValueOrThrow(), context, cancellationToken).DefaultAwait();
            else
                await None(context, cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TE, TKey, TValue}(in Maybe{KeyValuePair{TKey, TValue}}, Func{TKey, TValue, TE}, Func{TE})"/>
        public static async ValueTask<TE> Match<TE, TKey, TValue>(
            this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, CancellationToken, ValueTask<TE>> Some,
            Func<CancellationToken, ValueTask<TE>> None,
            CancellationToken cancellationToken = default
        )
        {
            return maybe.HasValue
                ? await Some.Invoke(
                    maybe.GetValueOrThrow().Key,
                    maybe.GetValueOrThrow().Value,
                    cancellationToken
                ).DefaultAwait()
                : await None.Invoke(cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TE, TKey, TValue, TContext}(in Maybe{KeyValuePair{TKey, TValue}}, Func{TKey, TValue, TContext, TE}, Func{TContext, TE}, TContext)"/>
        public static async ValueTask<TE> Match<TE, TKey, TValue, TContext>(
            this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, TContext, CancellationToken, ValueTask<TE>> Some,
            Func<TContext, CancellationToken, ValueTask<TE>> None,
            TContext context,
            CancellationToken cancellationToken = default
        )
        {
            return maybe.HasValue
                ? await Some.Invoke(
                    maybe.GetValueOrThrow().Key,
                    maybe.GetValueOrThrow().Value,
                    context,
                    cancellationToken
                ).DefaultAwait()
                : await None.Invoke(context, cancellationToken).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TKey, TValue}(in Maybe{KeyValuePair{TKey, TValue}}, Action{TKey, TValue}, Action)"/>
        public static async ValueTask Match<TKey, TValue>(
            this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, CancellationToken, ValueTask> Some,
            Func<CancellationToken, ValueTask> None,
            CancellationToken cancellationToken = default
        )
        {
            if (maybe.HasValue)
            {
                await Some.Invoke(
                    maybe.GetValueOrThrow().Key,
                    maybe.GetValueOrThrow().Value,
                    cancellationToken
                ).DefaultAwait();
            }
            else
            {
                await None.Invoke(cancellationToken).DefaultAwait();
            }
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.MaybeExtensions.Match{TKey, TValue, TContext}(in Maybe{KeyValuePair{TKey, TValue}}, Action{TKey, TValue, TContext}, Action{TContext}, TContext)"/>
        public static async ValueTask Match<TKey, TValue, TContext>(
            this Maybe<KeyValuePair<TKey, TValue>> maybe,
            Func<TKey, TValue, TContext, CancellationToken, ValueTask> Some,
            Func<TContext, CancellationToken, ValueTask> None,
            TContext context,
            CancellationToken cancellationToken = default
        )
        {
            if (maybe.HasValue)
            {
                await Some.Invoke(
                    maybe.GetValueOrThrow().Key,
                    maybe.GetValueOrThrow().Value,
                    context,
                    cancellationToken
                ).DefaultAwait();
            }
            else
            {
                await None.Invoke(context, cancellationToken).DefaultAwait();
            }
        }
    }
}
