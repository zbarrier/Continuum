#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string MinLengthSimpleError = "The length of '{0}' must be at least {1} characters.";

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(IEnumerable<T> value, int minLength, string propertyName)
    {
        return value.Count() < minLength
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(IEnumerable<T> value, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (value.Count() < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(string value, int minLength, string propertyName)
    {
        return value.Length < minLength
            ? Result.Failure<string>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(string value, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (value.Length < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength));
        }

        return Result.Success(value);
    }
}
