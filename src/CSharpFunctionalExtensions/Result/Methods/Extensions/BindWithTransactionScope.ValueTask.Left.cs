#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static ValueTask<Result<K>> BindWithTransactionScope<T, K>(this ValueTask<Result<T>> self, Func<T, Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        public static ValueTask<Result<K>> BindWithTransactionScope<K>(this ValueTask<Result> self, Func<Result<K>> f)
            => WithTransactionScope(() => self.Bind(f));

        public static ValueTask<Result> BindWithTransactionScope<T>(this ValueTask<Result<T>> self, Func<T, Result> f)
            => WithTransactionScope(() => self.Bind(f));

        public static ValueTask<Result> BindWithTransactionScope(this ValueTask<Result> self, Func<Result> f)
            => WithTransactionScope(() => self.Bind(f));
    }
}
#endif
