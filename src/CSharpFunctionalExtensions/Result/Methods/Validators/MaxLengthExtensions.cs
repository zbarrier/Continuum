#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string MaxLengthError = "The length of '{0}' must be {1} characters or fewer.";

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(this Result<IEnumerable<T>> result, int maxLength, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Count() > maxLength
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength))
            : result;
    }

    /// <summary>
    ///     Ensure collection count is less than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MaxLength<T>(this Result<IEnumerable<T>> result, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Count() > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(this Result<string> result, int maxLength, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Length > maxLength
            ? Result.Failure<string>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength))
            : result;
    }


    /// <summary>
    ///     Ensure string length is less than or equal to value.
    /// </summary>
    public static Result<string> MaxLength(this Result<string> result, int maxLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Length > maxLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, MaxLengthError, propertyName, maxLength));
        }

        return result;
    }
}
