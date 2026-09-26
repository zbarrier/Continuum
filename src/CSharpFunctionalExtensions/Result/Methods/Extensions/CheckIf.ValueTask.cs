#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class AsyncResultExtensionsBothOperands
    {
        public static ValueTask<Result<T>> CheckIf<T>(this ValueTask<Result<T>> resultTask, bool condition,
            Func<T, ValueTask<Result>> valueTask)
        {
            if (condition)
                return resultTask.Check(valueTask);
            else
                return resultTask;
        }

        public static ValueTask<Result<T>> CheckIf<T, K>(this ValueTask<Result<T>> resultTask, bool condition,
            Func<T, ValueTask<Result<K>>> valueTask)
        {
            if (condition)
                return resultTask.Check(valueTask);
            else
                return resultTask;
        }

        public static async ValueTask<Result<T>> CheckIf<T>(this ValueTask<Result<T>> resultTask,
            Func<T, bool> predicate, Func<T, ValueTask<Result>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(valueTask);
            else
                return result;
        }

        public static async ValueTask<Result<T>> CheckIf<T, K>(this ValueTask<Result<T>> resultTask,
            Func<T, bool> predicate, Func<T, ValueTask<Result<K>>> valueTask)
        {
            Result<T> result = await resultTask;

            if (result.IsSuccess && predicate(result.Value))
                return await result.Check(valueTask);
            else
                return result;
        }
    }
}
#endif
