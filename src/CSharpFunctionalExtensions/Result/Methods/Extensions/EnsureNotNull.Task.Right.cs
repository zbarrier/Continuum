#nullable enable

using System;
using System.Threading.Tasks;

#if NET40
using Task = System.Threading.Tasks.TaskEx;
#else
using Task = System.Threading.Tasks.Task;
#endif

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Task<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<Task<Error>> errorFactory)
            where T : class
        {
            return result.Ensure(value => Task.FromResult(value != null), _ => errorFactory()).Map(value => value!);
        }

        public static Task<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<Task<Error>> errorFactory)
            where T : struct
        {
            return result.Ensure(value => Task.FromResult(value != null), _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
