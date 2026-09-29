using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static ValueTask<Result<K>> BindWithTransactionScope<T, K>(this ValueTask<Result<T>> self, Func<T, Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{K}(Result, Func{Result{K}})"/>
        public static ValueTask<Result<K>> BindWithTransactionScope<K>(this ValueTask<Result> self, Func<Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope{T}(Result{T}, Func{T, Result})"/>
        public static ValueTask<Result> BindWithTransactionScope<T>(this ValueTask<Result<T>> self, Func<T, Result> f)
            => WithTransactionScope(() => self.Bind(f));

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindWithTransactionScope(Result, Func{Result})"/>
        public static ValueTask<Result> BindWithTransactionScope(this ValueTask<Result> self, Func<Result> f)
            => WithTransactionScope(() => self.Bind(f));
    }
}
