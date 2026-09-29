using Continuum.CSharpFunctionalExtensions.Internal;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Represents the outcome of an operation that returns a value of type <typeparamref name="T"/> on success, or an <see cref="Error"/> on failure.
/// </summary>
public readonly partial struct Result<T> : IResult<T>
{
    /// <inheritdoc/>
    public bool IsFailure { get; }
    /// <inheritdoc/>
    public bool IsSuccess => !IsFailure;

    private readonly Error? _error;
    /// <summary>
    ///     Gets the error of a failed result.
    /// </summary>
    /// <exception cref="ResultSuccessException">The result is successful.</exception>
    public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

    private readonly T? _value;
    /// <summary>
    ///     Gets the value of a successful result.
    /// </summary>
    /// <exception cref="ResultFailureException">The result is a failure.</exception>
    public T Value => IsSuccess ? _value! : throw new ResultFailureException(Error);

    internal Result(bool isFailure, Error? error, T? value)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
        _value = value;
    }

    /// <summary>
    ///     Returns the value if the result is successful; otherwise <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="defaultValue">The value to return on failure.</param>
    public T? GetValueOrDefault(T? defaultValue = default)
    {
        if (IsFailure)
            return defaultValue;

        return Value;
    }

    /// <summary>
    ///     Wraps <paramref name="value"/> in a successful result. If the value itself implements <see cref="IResult{T}"/>, its state is copied instead.
    /// </summary>
    public static implicit operator Result<T>(T value)
    {
        if (value is IResult<T> result)
        {
            Error? resultError = result.IsFailure ? result.Error : default;
            T? resultValue = result.IsSuccess ? result.Value : default;

            return new Result<T>(result.IsFailure, resultError, resultValue);
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Converts to a non-generic <see cref="Result"/>, discarding the value and preserving the error.
    /// </summary>
    public static implicit operator Result(Result<T> result)
    {
        if (result.IsSuccess)
            return Result.Success();
        else
            return Result.Failure(result.Error);
    }
}
