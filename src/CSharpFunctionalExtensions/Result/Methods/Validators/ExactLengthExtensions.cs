#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(this Result<IEnumerable<T>> result, int length, string propertyName)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count != length)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.ExactCount, ValidatorErrorStrings.ExactCount, propertyName, length, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(this Result<IEnumerable<T>> result, int length, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var count = result.Value.Count();
        if (count != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.ExactCount, ValidatorErrorStrings.ExactCount, propertyName, length, count));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(this Result<string> result, int length, string propertyName)
    {
        if (result.IsFailure) return result;

        var actualLength = result.Value.Length;
        if (actualLength != length)
        {
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExactLength, ValidatorErrorStrings.ExactLength, propertyName, length, actualLength));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(this Result<string> result, int length, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var actualLength = result.Value.Length;
        if (actualLength != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.ExactLength, ValidatorErrorStrings.ExactLength, propertyName, length, actualLength));
        }

        return result;
    }
}
