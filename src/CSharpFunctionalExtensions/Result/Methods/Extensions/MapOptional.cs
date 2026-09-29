namespace Continuum.CSharpFunctionalExtensions;
public static partial class ResultExtensions
{
    /// <summary>
    ///     Maps the inner value of a <see cref="Result{T}"/> of <see cref="Maybe{T}"/>, staying optional.
    ///     The function is only called when the result is successful and the <see cref="Maybe{T}"/> has a value.
    /// </summary>
    /// <typeparam name="T">The type of the inner value.</typeparam>
    /// <typeparam name="K">The type of the mapped value.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="func">The function to apply to the inner value.</param>
    /// <returns>
    ///     The original failure; a successful result with <see cref="Maybe.None"/> when there is no value;
    ///     otherwise a successful result containing the mapped value.
    /// </returns>
    public static Result<Maybe<K>> MapOptional<T, K>(
        this Result<Maybe<T>> result,
        Func<T, K> func)
    {
        if (result.IsFailure)
            return Result.Failure<Maybe<K>>(result.Error);

        return Result.Success(result.Value.Map(func));
    }
}
