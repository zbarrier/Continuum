using System;
using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T, K}(Result{T}, Func{T, Result{K}})"/>
        public static async Task<Result<(T First, K Second)>> BindZip<T, K>(
            this Task<Result<T>> resultTask, Func<T, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure ? Result.Failure<(T, K)>(result.Error) : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static async Task<Result<(T1 First, T2 Second, K Third)>> BindZip<T1, T2, K>(
            this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, K)>(result.Error) : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, T3, K}(Result{ValueTuple{T1, T2, T3}}, Func{T1, T2, T3, Result{K}})"/>
        public static async Task<Result<(T1, T2, T3, K)>> BindZip<T1, T2, T3, K>(
            this Task<Result<(T1, T2, T3)>> resultTask, Func<T1, T2, T3, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, K)>(result.Error)
                : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, T3, T4, K}(Result{ValueTuple{T1, T2, T3, T4}}, Func{T1, T2, T3, T4, Result{K}})"/>
        public static async Task<Result<(T1, T2, T3, T4, K)>> BindZip<T1, T2, T3, T4, K>(
            this Task<Result<(T1, T2, T3, T4)>> resultTask, Func<T1, T2, T3, T4, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, K)>(result.Error)
                : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, T3, T4, T5, K}(Result{ValueTuple{T1, T2, T3, T4, T5}}, Func{T1, T2, T3, T4, T5, Result{K}})"/>
        public static async Task<Result<(T1, T2, T3, T4, T5, K)>> BindZip<T1, T2, T3, T4, T5, K>(
            this Task<Result<(T1, T2, T3, T4, T5)>> resultTask, Func<T1, T2, T3, T4, T5, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, K)>(result.Error)
                : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, T3, T4, T5, T6, K}(Result{ValueTuple{T1, T2, T3, T4, T5, T6}}, Func{T1, T2, T3, T4, T5, T6, Result{K}})"/>
        public static async Task<Result<(T1, T2, T3, T4, T5, T6, K)>> BindZip<T1, T2, T3, T4, T5, T6, K>(
            this Task<Result<(T1, T2, T3, T4, T5, T6)>> resultTask,
            Func<T1, T2, T3, T4, T5, T6, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, K)>(result.Error)
                : result.BindZip(func);
        }

        /// <inheritdoc cref="Continuum.CSharpFunctionalExtensions.ResultExtensions.BindZip{T1, T2, T3, T4, T5, T6, T7, K}(Result{ValueTuple{T1, T2, T3, T4, T5, T6, T7}}, Func{T1, T2, T3, T4, T5, T6, T7, Result{K}})"/>
        public static async Task<Result<(T1, T2, T3, T4, T5, T6, T7, K)>> BindZip<T1, T2, T3, T4, T5, T6, T7, K>(
            this Task<Result<(T1, T2, T3, T4, T5, T6, T7)>> resultTask,
            Func<T1, T2, T3, T4, T5, T6, T7, Result<K>> func
        ) {
            var result = await resultTask.DefaultAwait();

            return result.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, T7, K)>(result.Error)
                : result.BindZip(func);
        }
    }
}
