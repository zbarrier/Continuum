using System;
using System.Threading.Tasks;
using System.Transactions;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        private static readonly TransactionOptions _transactionOptions = new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted,
            Timeout = TransactionManager.DefaultTimeout
        };

        private static T WithTransactionScope<T>(Func<T> f)
            where T : IResult
        {
            using (var trans = new TransactionScope(TransactionScopeOption.Required, _transactionOptions,
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = f();
                if (result.IsSuccess)
                {
                    trans.Complete();
                }

                return result;
            }
        }

        private static async Task<T> WithTransactionScope<T>(Func<Task<T>> f)
            where T : IResult
        {
            using (var trans = new TransactionScope(TransactionScopeOption.Required, _transactionOptions,
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await f().DefaultAwait();
                if (result.IsSuccess)
                {
                    trans.Complete();
                }

                return result;
            }
        }
    }
}

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        private static async ValueTask<T> WithTransactionScope<T>(Func<ValueTask<T>> f)
            where T : IResult
        {
            using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = await f();
                if (result.IsSuccess)
                {
                    trans.Complete();
                }

                return result;
            }
        }
    }
}
