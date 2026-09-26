#if (NETSTANDARD || NETCORE || NET5_0_OR_GREATER)
using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        public static Task<Result<(T First, K Second)>> BindZip<T, K>(
            this Result<T> result, Func<T, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1 First, T2 Second, K Third)>> BindZip<T1, T2, K>(
            this Result<(T1, T2)> result, Func<T1, T2, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1, T2, T3, K)>> BindZip<T1, T2, T3, K>(
            this Result<(T1, T2, T3)> result, Func<T1, T2, T3, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1, T2, T3, T4, K)>> BindZip<T1, T2, T3, T4, K>(
            this Result<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1, T2, T3, T4, T5, K)>> BindZip<T1, T2, T3, T4, T5, K>(
            this Result<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1, T2, T3, T4, T5, T6, K)>> BindZip<T1, T2, T3, T4, T5, T6, K>(
            this Result<(T1, T2, T3, T4, T5, T6)> result, Func<T1, T2, T3, T4, T5, T6, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }

        public static Task<Result<(T1, T2, T3, T4, T5, T6, T7, K)>> BindZip<T1, T2, T3, T4, T5, T6, T7, K>(
            this Result<(T1, T2, T3, T4, T5, T6, T7)> result,
            Func<T1, T2, T3, T4, T5, T6, T7, Task<Result<K>>> func
        ) {
            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, T7, K)>(result.Error).AsCompletedTask()
                : result.AsCompletedTask().BindZip(func);
        }
    }
}
#endif
