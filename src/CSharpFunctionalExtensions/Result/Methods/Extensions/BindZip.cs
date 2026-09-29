using System;

namespace Continuum.CSharpFunctionalExtensions
{
    public static partial class ResultExtensions
    {
        /// <summary>
        ///     Selects result from the return value of a given function. If the calling Result is a success,
        ///     values of both results zip into a tuple. If the calling Result is a failure, a new failure
        ///     result is returned instead.
        /// </summary>
        /// <typeparam name="T">The type of the source value.</typeparam>
        /// <typeparam name="K">The type of the value produced by <paramref name="func"/>.</typeparam>
        /// <param name="result">The source result.</param>
        /// <param name="func">Produces the next result from the source value.</param>
        /// <returns>A tuple of the source value and the bound value, or the first failure encountered.</returns>
        public static Result<(T First, K Second)> BindZip<T, K>(
            this Result<T> result, Func<T, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T, K)>(result.Error);
            }

            var result2 = func(result.Value);

            return result2.IsFailure
                ? Result.Failure<(T, K)>(result2.Error)
                : Result.Success((result.Value, result2.Value));
        }
        
        /// <summary>
        ///     Binds a tuple result and appends the bound value to the tuple. If either result is a failure, that failure is returned.
        /// </summary>
        /// <param name="result">The source tuple result.</param>
        /// <param name="func">Produces the next result from the tuple items.</param>
        /// <returns>The source tuple extended with the bound value, or the first failure encountered.</returns>
        public static Result<(T1 First, T2 Second, K Third)> BindZip<T1, T2, K>(
            this Result<(T1, T2)> result, Func<T1, T2, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, K)>(result.Error);
            }

            var v  = result.Value;
            var r2 = func(v.Item1, v.Item2);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, r2.Value));
        }
        
        /// <inheritdoc cref="BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static Result<(T1, T2, T3, K)> BindZip<T1, T2, T3, K>(
            this Result<(T1, T2, T3)> result, Func<T1, T2, T3, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, T3, K)>(result.Error);
            }

            var v  = result.Value;
            var r2 = func(v.Item1, v.Item2, v.Item3);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, T3, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, v.Item3, r2.Value));
        }

        /// <inheritdoc cref="BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static Result<(T1, T2, T3, T4, K)> BindZip<T1, T2, T3, T4, K>(
            this Result<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, T3, T4, K)>(result.Error);
            }

            var v  = result.Value;
            var r2 = func(v.Item1, v.Item2, v.Item3, v.Item4);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, v.Item3, v.Item4, r2.Value));
        }
        
        /// <inheritdoc cref="BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static Result<(T1, T2, T3, T4, T5, K)> BindZip<T1, T2, T3, T4, T5, K>(
            this Result<(T1, T2, T3, T4, T5)> result, Func<T1, T2, T3, T4, T5, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, T3, T4, T5, K)>(result.Error);
            }

            var v = result.Value;
            var r2 = func(v.Item1, v.Item2, v.Item3, v.Item4, v.Item5);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, r2.Value));
        }

        /// <inheritdoc cref="BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static Result<(T1, T2, T3, T4, T5, T6, K)> BindZip<T1, T2, T3, T4, T5, T6, K>(
            this Result<(T1, T2, T3, T4, T5, T6)> result, Func<T1, T2, T3, T4, T5, T6, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, T3, T4, T5, T6, K)>(result.Error);
            }

            var v  = result.Value;
            var r2 = func(v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6, r2.Value));
        }

        /// <inheritdoc cref="BindZip{T1, T2, K}(Result{ValueTuple{T1, T2}}, Func{T1, T2, Result{K}})"/>
        public static Result<(T1, T2, T3, T4, T5, T6, T7, K)> BindZip<T1, T2, T3, T4, T5, T6, T7, K>(
            this Result<(T1, T2, T3, T4, T5, T6, T7)> result, Func<T1, T2, T3, T4, T5, T6, T7, Result<K>> func
        ) {
            if (result.IsFailure)
            {
                return Result.Failure<(T1, T2, T3, T4, T5, T6, T7, K)>(result.Error);
            }

            var v  = result.Value;
            var r2 = func(v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6, v.Item7);

            return r2.IsFailure
                ? Result.Failure<(T1, T2, T3, T4, T5, T6, T7, K)>(r2.Error)
                : Result.Success((v.Item1, v.Item2, v.Item3, v.Item4, v.Item5, v.Item6, v.Item7, r2.Value));
        }
    }
}
