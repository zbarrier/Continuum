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
    public static Result<IEnumerable<T>> MaxLength<T>(IEnumerable<T> value, int maxLength, PropertyPath path, string member)
    {
        var count = value.Count();
        if (count > maxLength)
        {
            var propertyName = path.Format(member);
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
    public static Result<string> MaxLength(string value, int maxLength, PropertyPath path, string member)
    {
        var length = value.Length;
        if (length > maxLength)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MaxLength, ValidatorErrorStrings.MaxLength, propertyName, maxLength, length));
        }

        return Result.Success(value);
    }
}
