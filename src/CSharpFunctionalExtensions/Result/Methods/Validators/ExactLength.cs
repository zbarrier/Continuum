#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string ExactLengthSimpleError = "'{0}' must be {1} characters in length.";

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(IEnumerable<T> value, int length, string propertyName)
    {
        return value.Count() != length
            ? Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is equal to value.
    /// </summary>
    public static Result<IEnumerable<T>> ExactLength<T>(IEnumerable<T> value, int length, string propertyNameFormat, params object[] arguments)
    {
        if (value.Count() != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(string value, int length, string propertyName)
    {
        return value.Length != length
            ? Result.Failure<string>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length))
            : Result.Success(value);
    }

    /// <summary>
    ///     Ensure string length is equal to value.
    /// </summary>
    public static Result<string> ExactLength(string value, int length, string propertyNameFormat, params object[] arguments)
    {
        if (value.Length != length)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ExactLengthSimpleError, propertyName, length));
        }

        return Result.Success(value);
    }
}
