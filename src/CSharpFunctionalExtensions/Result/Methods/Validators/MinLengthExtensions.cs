#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string MinLengthSimpleError = "The length of '{0}' must be at least {1} characters.";

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(this Result<IEnumerable<T>> result, int minLength, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Count() < minLength
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength))
            : result;
    }

    /// <summary>
    ///     Ensure collection count is greater than or equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> MinLength<T>(this Result<IEnumerable<T>> result, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Count() < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(this Result<string> result, int minLength, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Length < minLength
            ? Result.Failure<string>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength))
            : result;
    }

    /// <summary>
    ///     Ensure string length is greater than or equal to value.
    /// </summary>
    public static Result<string> MinLength(this Result<string> result, int minLength, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Length < minLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, MinLengthSimpleError, propertyName, minLength));
        }

        return result;
    }
}
