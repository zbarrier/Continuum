#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        return DateTime.TryParse(result.Value, out DateTime dtValue)
            ? Result.Success(dtValue)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!DateTime.TryParse(result.Value, out DateTime dtValue))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(dtValue);
    }
}
