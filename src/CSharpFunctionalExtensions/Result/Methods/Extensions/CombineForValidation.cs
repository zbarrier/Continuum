namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     If the <paramref name="result"/> is a success, combines several results (and any validation error messages) into a single result.
    ///     Otherwise, returns the failed <paramref name="result"/> without combining.
    /// </summary>
    /// <inheritdoc cref="Result.CombineForValidation(IEnumerable{Result})"/>
    public static Result CombineForValidation(this Result result, IEnumerable<Result> results)
        => result.IsFailure ? result : Result.CombineForValidation(results);

    /// <summary>
    ///     If the <paramref name="result"/> is a success, combines several results (and any validation error messages) into a single result.
    ///     Otherwise, returns the failed <paramref name="result"/> without combining.
    /// </summary>
    /// <inheritdoc cref="Result.CombineForValidation{T}(IEnumerable{Result{T}})"/>
    public static Result CombineForValidation<T>(this Result result, IEnumerable<Result<T>> results)
        => result.IsFailure ? result : Result.CombineForValidation(results);

    /// <summary>
    ///     If the <paramref name="result"/> is a success, combines several results (and any validation error messages) into a single result.
    ///     Otherwise, returns the failed <paramref name="result"/> without combining.
    /// </summary>
    /// <inheritdoc cref="Result.CombineForValidation(Result[])"/>
    public static Result CombineForValidation(this Result result, params Result[] results)
        => result.IsFailure ? result : Result.CombineForValidation(results);

    /// <summary>
    ///     If the <paramref name="result"/> is a success, combines several results (and any validation error messages) into a single result.
    ///     Otherwise, returns the failed <paramref name="result"/> without combining.
    /// </summary>
    /// <inheritdoc cref="Result.CombineForValidation{T}(Result{T}[])"/>
    public static Result CombineForValidation<T>(this Result result, params Result<T>[] results)
        => result.IsFailure ? result : Result.CombineForValidation(results);
}
