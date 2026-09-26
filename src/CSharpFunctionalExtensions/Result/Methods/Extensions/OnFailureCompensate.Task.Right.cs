using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class AsyncResultExtensionsRightOperand
    {
        public static async Task<Result<T>> OnFailureCompensate<T>(this Result<T> result, Func<Task<Result<T>>> func)
        {
            if (result.IsFailure)
                return await func().DefaultAwait();

            return result;
        }

        public static async Task<Result> OnFailureCompensate(this Result result, Func<Task<Result>> func)
        {
            if (result.IsFailure)
                return await func().DefaultAwait();

            return result;
        }

        public static async Task<Result<T>> OnFailureCompensate<T>(this Result<T> result, Func<Error, Task<Result<T>>> func)
        {
            if (result.IsFailure)
                return await func(result.Error).DefaultAwait();

            return result;
        }
        
        public static async Task<Result> OnFailureCompensate(this Result result, Func<Error, Task<Result>> func)
        {
            if (result.IsFailure)
                return await func(result.Error).DefaultAwait();

            return result;
        }
    }
}
