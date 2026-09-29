#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(IEnumerable<T> value, int maxLength, string propertyName)
    {
        var count = value.Count();
        if (count > maxLength)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MaxCount, ValidatorErrorStrings.MaxCount, propertyName, maxLength, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(IEnumerable<T> value, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        var count = value.Count();
        if (count > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MaxCount, ValidatorErrorStrings.MaxCount, propertyName, maxLength, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(string value, int maxLength, string propertyName)
    {
        var length = value.Length;
        if (length > maxLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MaxLength, ValidatorErrorStrings.MaxLength, propertyName, maxLength, length));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(string value, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        var length = value.Length;
        if (length > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MaxLength, ValidatorErrorStrings.MaxLength, propertyName, maxLength, length));
        }

        return Result.Success(value);
    }
}
