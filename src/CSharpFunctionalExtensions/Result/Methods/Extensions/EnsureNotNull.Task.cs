#nullable enable

using System;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Error)"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Task<Result<T?>> resultTask, Error error)
            where T : class
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Error)"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Task<Result<T?>> resultTask, Error error)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!.Value);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Task<Result<T?>> resultTask, Func<Task<Error>> errorFactory)
            where T : class
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static Task<Result<T>> EnsureNotNull<T>(this Task<Result<T?>> resultTask, Func<Task<Error>> errorFactory)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
