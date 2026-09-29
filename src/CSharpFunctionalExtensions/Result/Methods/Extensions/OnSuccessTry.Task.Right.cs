using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry(Result, Func{Task}, Func{Exception, Error})"/>
        public static async Task<Result> OnSuccessTry(this Result result, Func<Task> func,
            Func<Exception, Error>? errorHandler = null)
        {
            return result.IsFailure
                ? result
                : await Result.Try(func, errorHandler).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry{T}(Result{T}, Action{T}, Func{Exception, Error})"/>
        public static async Task<Result> OnSuccessTry<T>(this Result<T> result, Func<T, Task> func,
            Func<Exception, Error>? errorHandler = null)
        {
            return result.IsFailure
                ? Result.Failure(result.Error)
                : await Result.Try(() => func.Invoke(result.Value), errorHandler).DefaultAwait();
        }
    }
}
