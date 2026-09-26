#if NET5_0_OR_GREATER
#nullable enable

using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Error error)
            where T : class
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!);
        }

        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Error error)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!.Value);
        }

        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Func<ValueTask<Error>> errorFactory)
            where T : class
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!);
        }

        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Func<ValueTask<Error>> errorFactory)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
#endif