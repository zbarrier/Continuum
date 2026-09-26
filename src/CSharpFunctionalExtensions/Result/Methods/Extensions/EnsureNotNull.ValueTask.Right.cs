#if NET5_0_OR_GREATER
#nullable enable

using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        public static ValueTask<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<ValueTask<Error>> errorFactory)
            where T : class
        {
            return result.Ensure(value => ValueTask.FromResult(value != null), _ => errorFactory()).Map(value => value!);
        }

        public static ValueTask<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<ValueTask<Error>> errorFactory)
            where T : struct
        {
            return result.Ensure(value => ValueTask.FromResult(value != null), _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
#endif