#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(this Result<IEnumerable<T>> result, int maxLength, string propertyName)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count > maxLength)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MaxCount, ValidatorErrorStrings.MaxCount, propertyName, maxLength, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(this Result<IEnumerable<T>> result, int maxLength, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count > maxLength)
        {
            var propertyName = path.Format(member);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MaxCount, ValidatorErrorStrings.MaxCount, propertyName, maxLength, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(this Result<string> result, int maxLength, string propertyName)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length > maxLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MaxLength, ValidatorErrorStrings.MaxLength, propertyName, maxLength, length));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(this Result<string> result, int maxLength, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length > maxLength)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MaxLength, ValidatorErrorStrings.MaxLength, propertyName, maxLength, length));
        }

        return result;
    }
}
