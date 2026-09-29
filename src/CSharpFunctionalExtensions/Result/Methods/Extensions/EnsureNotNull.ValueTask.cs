#nullable enable

using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Error)"/>
        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Error error)
            where T : class
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Error)"/>
        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Error error)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, error).Map(value => value!.Value);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Func<ValueTask<Error>> errorFactory)
            where T : class
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.EnsureNotNull{T}(Result{T}, Func{Error})"/>
        public static ValueTask<Result<T>> EnsureNotNull<T>(this ValueTask<Result<T?>> resultTask, Func<ValueTask<Error>> errorFactory)
            where T : struct
        {
            return resultTask.Ensure(value => value != null, _ => errorFactory()).Map(value => value!.Value);
        }
    }
}
