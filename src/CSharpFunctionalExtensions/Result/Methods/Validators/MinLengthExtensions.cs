#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(this Result<IEnumerable<T>> result, int minLength, string propertyName)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count < minLength)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MinCount, ValidatorErrorStrings.MinCount, propertyName, minLength, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(this Result<IEnumerable<T>> result, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.MinCount, ValidatorErrorStrings.MinCount, propertyName, minLength, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(this Result<string> result, int minLength, string propertyName)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length < minLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MinLength, ValidatorErrorStrings.MinLength, propertyName, minLength, length));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(this Result<string> result, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var length = result.Value.Length;
        if (length < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.MinLength, ValidatorErrorStrings.MinLength, propertyName, minLength, length));
        }

        return result;
    }
}
