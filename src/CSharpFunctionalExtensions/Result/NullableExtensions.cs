using System.Threading.Tasks;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Extension methods for converting nullable values to results.
/// </summary>
public static partial class NullableExtensions
{
    /// <summary>
    ///     Converts a nullable value to a <see cref="Result{T}"/>: success if it has a value; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="nullable">The nullable value.</param>
    /// <param name="error">The error to use when there is no value.</param>
    public static Result<T> ToResult<T>(in this T? nullable, Error error)
        where T : struct
    {
        if (!nullable.HasValue)
            return Result.Failure<T>(error);

        return Result.Success<T>(nullable.Value);
    }
    /// <summary>
    ///     Converts a possibly-null reference to a <see cref="Result{T}"/>: success if it is not null; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="obj">The possibly-null reference.</param>
    /// <param name="error">The error to use when the reference is null.</param>
    public static Result<T> ToResult<T>(this T? obj, Error error)
        where T : class
    {
        if (obj == null)
            return Result.Failure<T>(error);

        return Result.Success<T>(obj);
    }

    /// <summary>
    ///     Awaits a nullable value and converts it to a <see cref="Result{T}"/>: success if it has a value; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="nullableTask">A task producing the nullable value.</param>
    /// <param name="error">The error to use when there is no value.</param>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> nullableTask, Error error)
        where T : struct
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(error);
    }

    /// <summary>
    ///     Awaits a possibly-null reference and converts it to a <see cref="Result{T}"/>: success if it is not null; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="nullableTask">A task producing the possibly-null reference.</param>
    /// <param name="error">The error to use when there is no value.</param>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> nullableTask, Error error)
    where T : class
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(error);
    }

    /// <summary>
    ///     Awaits a nullable value and converts it to a <see cref="Result{T}"/>: success if it has a value; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="nullableTask">A task producing the nullable value.</param>
    /// <param name="error">The error to use when there is no value.</param>
    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<T?> nullableTask, Error error)
        where T : struct
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(error);
    }

    /// <summary>
    ///     Awaits a possibly-null reference and converts it to a <see cref="Result{T}"/>: success if it is not null; otherwise failure with <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="nullableTask">A task producing the possibly-null reference.</param>
    /// <param name="error">The error to use when there is no value.</param>
    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<T?> nullableTask, Error error)
        where T : class
    {
        var nullable = await nullableTask.ConfigureAwait(false);
        return nullable.ToResult(error);
    }
}
