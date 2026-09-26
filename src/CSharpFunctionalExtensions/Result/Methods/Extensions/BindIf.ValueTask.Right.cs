#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static ValueTask<Result> BindIf(this Result result, bool condition, Func<ValueTask<Result>> valueTask)
        {
            if (!condition)
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        public static ValueTask<Result<T>> BindIf<T>(this Result<T> result, bool condition, Func<T, ValueTask<Result<T>>> valueTask)
        {
            if (!condition)
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        public static ValueTask<Result> BindIf(this Result result, Func<bool> predicate, Func<ValueTask<Result>> valueTask)
        {
            if (!result.IsSuccess || !predicate())
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        public static ValueTask<Result<T>> BindIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, ValueTask<Result<T>>> valueTask)
        {
            if (!result.IsSuccess || !predicate(result.Value))
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }
    }
}
#endif