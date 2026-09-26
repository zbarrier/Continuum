#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string MaxLengthError = "The length of '{0}' must be {1} characters or fewer.";

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(IEnumerable<T> value, int maxLength, string propertyName)
    => value.Count() > maxLength
        ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength))
        : Result.Success(value);

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(IEnumerable<T> value, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        if (value.Count() > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(string value, int maxLength, string propertyName)
    => value.Length > maxLength
        ? Result.Failure<string>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength))
        : Result.Success(value);

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(string value, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        if (value.Length > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength));
        }

        return Result.Success(value);
    }
}
