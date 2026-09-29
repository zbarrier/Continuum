#nullable enable

using System;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<Task<Error>> errorFactory)
            where T : class
        {
            return result.Ensure(value => Task.FromResult(value != null), _ => errorFactory()).Map(value => value!);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Result<T?> result, Func<Task<Error>> errorFactory)
            where T : struct
        {
            return result.Ensure(value => Task.FromResult(value != null), _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
