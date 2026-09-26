using Continuum.CSharpFunctionalExtensions.Internal;

namespace Continuum.CSharpFunctionalExtensions;

public readonly partial struct Result<T> : IResult<T>
{
    public bool IsFailure { get; }
    public bool IsSuccess => !IsFailure;

    private readonly Error _error;
    public Error Error => ResultCommonLogic.GetErrorWithSuccessGuard(IsFailure, _error);

    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new ResultFailureException(Error);

    internal Result(bool isFailure, Error error, T value)
    {
        IsFailure = ResultCommonLogic.ErrorStateGuard(isFailure, error);
        _error = error;
        _value = value;
    }

    public T GetValueOrDefault(T defaultValue = default)
    {
        if (IsFailure)
            return defaultValue;

        return Value;
    }

    public static implicit operator Result<T>(T value)
    {
        if (value is IResult<T> result)
        {
            Error resultError = result.IsFailure ? result.Error : default;
            T resultValue = result.IsSuccess ? result.Value : default;

            return new Result<T>(result.IsFailure, resultError, resultValue);
        }

        return Result.Success(value);
    }

    public static implicit operator Result(Result<T> result)
    {
        if (result.IsSuccess)
            return Result.Success();
        else
            return Result.Failure(result.Error);
    }
}
