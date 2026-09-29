#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure collection count is between min and max (inclusive).
    /// </summary>
    public static Result<IEnumerable<T>> CountBetween<T>(IEnumerable<T> value, int min, int max, string propertyName)
    {
        var count = value.Count();
        if (count < min || count > max)
        {
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.CountBetween, ValidatorErrorStrings.CountBetween, propertyName, min, max, count));
        }

        return Result.Success(value);
    }

    /// <summary>
    ///     Ensure collection count is between min and max (inclusive).
    /// </summary>
    public static Result<IEnumerable<T>> CountBetween<T>(IEnumerable<T> value, int min, int max, string propertyNameFormat, params object[] arguments)
    {
        var count = value.Count();
        if (count < min || count > max)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<IEnumerable<T>>(new ValidationError(propertyName, ValidationErrorCodes.CountBetween, ValidatorErrorStrings.CountBetween, propertyName, min, max, count));
        }

        return Result.Success(value);
    }
}
