#if NETSTANDARD2_0 || NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Task<Result<K>> BindWithTransactionScope<T, K>(this Result<T> self, Func<T, Task<Result<K>>> f)
            => WithTransactionScope(() => self.Bind(f));

        public static Task<Result<K>> BindWithTransactionScope<K>(this Result self, Func<Task<Result<K>>> f)
            => WithTransactionScope(() => self.Bind(f));

        public static Task<Result> BindWithTransactionScope<T>(this Result<T> self, Func<T, Task<Result>> f)
            => WithTransactionScope(() => self.Bind(f));

        public static Task<Result> BindWithTransactionScope(this Result self, Func<Task<Result>> f)
            => WithTransactionScope(() => self.Bind(f));
    }
}
#endif
