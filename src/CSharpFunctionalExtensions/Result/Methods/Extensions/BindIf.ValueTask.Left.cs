#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, bool condition, Func<Result> valueTask)
        {
            var result = await resultTask;
            return result.BindIf(condition, valueTask);
        }

        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, bool condition, Func<T, Result<T>> valueTask)
        {
            var result = await resultTask;
            return result.BindIf(condition, valueTask);
        }

        public static async ValueTask<Result> BindIf(this ValueTask<Result> resultTask, Func<bool> predicate, Func<Result> valueTask)
        {
            var result = await resultTask;
            return result.BindIf(predicate, valueTask);
        }

        public static async ValueTask<Result<T>> BindIf<T>(this ValueTask<Result<T>> resultTask, Func<T, bool> predicate, Func<T, Result<T>> valueTask)
        {
            var result = await resultTask;
            return result.BindIf(predicate, valueTask);
        }
    }
}
#endif