using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry(Result, Func{Task}, Func{Exception, Error})"/>
        public static async Task<Result> OnSuccessTry(this Task<Result> task, Func<Task> func,
            Func<Exception, Error>? errorHandler = null)
        {
            var result = await task.DefaultAwait();
            return await result.OnSuccessTry(func, errorHandler).DefaultAwait();
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.OnSuccessTry{T}(Result{T}, Action{T}, Func{Exception, Error})"/>
        public static async Task<Result> OnSuccessTry<T>(this Task<Result<T>> task, Func<T, Task> func,
            Func<Exception, Error>? errorHandler = null)
        {
            var result = await task.DefaultAwait();
            return await result.OnSuccessTry(func, errorHandler).DefaultAwait();
        }
    }
}
