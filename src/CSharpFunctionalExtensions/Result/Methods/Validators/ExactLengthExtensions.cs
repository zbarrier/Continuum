#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string ExactLengthSimpleError = "'{0}' must be {1} characters in length.";

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(this Result<IEnumerable<T>> result, int length, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Count() != length
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length))
            : result;
    }

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(this Result<IEnumerable<T>> result, int length, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Count() != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length));
        }

        return result;
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(this Result<string> result, int length, string propertyName)
    {
        if (result.IsFailure) return result;

        return result.Value.Length != length
            ? Result.Failure<string>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length))
            : result;
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(this Result<string> result, int length, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (result.Value.Length != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length));
        }

        return result;
    }
}
