using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static Task<Result<K>> BindWithTransactionScope<T, K>(this Task<Result<T>> self, Func<T, Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{K}(Result, Func{Result{K}})"/>
        public static Task<Result<K>> BindWithTransactionScope<K>(this Task<Result> self, Func<Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{T}(Result{T}, Func{T, Result})"/>
        public static Task<Result> BindWithTransactionScope<T>(this Task<Result<T>> self, Func<T, Result> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope(Result, Func{Result})"/>
        public static Task<Result> BindWithTransactionScope(this Task<Result> self, Func<Result> f)
            => WithTransactionScope(() => self.Bind(f));
    }
}
