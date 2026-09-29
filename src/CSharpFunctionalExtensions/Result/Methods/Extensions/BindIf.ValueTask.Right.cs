using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions.ValueTasks
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, bool, Func{Result})"/>
        public static ValueTask<Result> BindIf(this Result result, bool condition, Func<ValueTask<Result>> valueTask)
        {
            if (!condition)
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, bool, Func{T, Result{T}})"/>
        public static ValueTask<Result<T>> BindIf<T>(this Result<T> result, bool condition, Func<T, ValueTask<Result<T>>> valueTask)
        {
            if (!condition)
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf(Result, Func{bool}, Func{Result})"/>
        public static ValueTask<Result> BindIf(this Result result, Func<bool> predicate, Func<ValueTask<Result>> valueTask)
        {
            if (!result.IsSuccess || !predicate())
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindIf{T}(Result{T}, Func{T, bool}, Func{T, Result{T}})"/>
        public static ValueTask<Result<T>> BindIf<T>(this Result<T> result, Func<T, bool> predicate, Func<T, ValueTask<Result<T>>> valueTask)
        {
            if (!result.IsSuccess || !predicate(result.Value))
            {
                return result.AsCompletedValueTask();
            }

            return result.Bind(valueTask);
        }
    }
}
