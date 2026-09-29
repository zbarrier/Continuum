using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Executes <c>Bind</c> inside a <see cref="System.Transactions.TransactionScope"/>, completing the scope only when the resulting operation succeeds.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the bound value.</typeparam>
        /// <param name="self">The source result.</param>
        /// <param name="f">The binding function.</param>
        /// <returns>The bound result.</returns>
        public static Result<K> BindWithTransactionScope<T, K>(this Result<T> self, Func<T, Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="BindWithTransactionScope{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static Result<K> BindWithTransactionScope<K>(this Result self, Func<Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="BindWithTransactionScope{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static Result BindWithTransactionScope<T>(this Result<T> self, Func<T, Result> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="BindWithTransactionScope{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static Result BindWithTransactionScope(this Result self, Func<Result> f)
            => WithTransactionScope(() => self.Bind(f));
    }
}
