using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsLeftOperand
    {
        public static Task<Result<T>> CheckIf<T>(this Task<Result<T>> resultTask, bool condition, Func<T, Result> func)
        {
            if (condition)
                return resultTask.Check(func);
            else
                return resultTask;
        }

        public static Task<Result<T>> CheckIf<T, K>(this Task<Result<T>> resultTask, bool condition, Func<T, Result<K>> func)
        {
            if (condition)
                return resultTask.Check(func);
            else
                return resultTask;
        }

        public static async Task<Result<T>> CheckIf<T>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Result> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return result;
        }

        public static async Task<Result<T>> CheckIf<T, K>(this Task<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Result<K>> func)
        {
            Result<T> result = await resultTask.DefaultAwait();

            if (result.IsSuccess && predicate(result.Value))
                return result.Check(func);
            else
                return result;
        }
    }
}
