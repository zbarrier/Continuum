#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        return TimeOnly.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        if (!TimeOnly.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
