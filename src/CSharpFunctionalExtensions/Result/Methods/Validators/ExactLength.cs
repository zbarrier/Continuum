#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(IEnumerable<T> value, int length, string propertyName)
    {
        var count = value.Count();
        if (count != length)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.ExactCount, ValidatorErrorStrings.ExactCount, propertyName, length, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(IEnumerable<T> value, int length, string propertyNameFormat, params object[] arguments)
    {
        var count = value.Count();
        if (count != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.ExactCount, ValidatorErrorStrings.ExactCount, propertyName, length, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(string value, int length, string propertyName)
    {
        var actualLength = value.Length;
        if (actualLength != length)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExactLength, ValidatorErrorStrings.ExactLength, propertyName, length, actualLength));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(string value, int length, string propertyNameFormat, params object[] arguments)
    {
        var actualLength = value.Length;
        if (actualLength != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExactLength, ValidatorErrorStrings.ExactLength, propertyName, length, actualLength));
        }

        return Result.Success(value);
    }
}
