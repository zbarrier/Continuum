#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, bool condition, Func<ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(condition, valueTask);
        }

        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<T, ValueTask<Result<T>>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(condition, valueTask);
        }

        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, Func<bool> predicate, Func<ValueTask<Result>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(predicate, valueTask);
        }

        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Func<T, ValueTask<Result<T>>> valueTask)
        {
            var result = await resultTask;
            return await result.BindIf(predicate, valueTask);
        }
    }
}
#endif