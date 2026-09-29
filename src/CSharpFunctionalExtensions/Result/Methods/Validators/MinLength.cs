#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(IEnumerable<T> value, int minLength, string propertyName)
    {
        var count = value.Count();
        if (count < minLength)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MinCount, ValidatorErrorStrings.MinCount, propertyName, minLength, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(IEnumerable<T> value, int minLength, string propertyNameFormat, params object[] arguments)
    {
        var count = value.Count();
        if (count < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MinCount, ValidatorErrorStrings.MinCount, propertyName, minLength, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(string value, int minLength, string propertyName)
    {
        var length = value.Length;
        if (length < minLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MinLength, ValidatorErrorStrings.MinLength, propertyName, minLength, length));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(string value, int minLength, string propertyNameFormat, params object[] arguments)
    {
        var length = value.Length;
        if (length < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MinLength, ValidatorErrorStrings.MinLength, propertyName, minLength, length));
        }

        return Result.Success(value);
    }
}
