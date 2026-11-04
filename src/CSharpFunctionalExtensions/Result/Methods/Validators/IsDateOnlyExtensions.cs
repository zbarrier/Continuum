#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        return DateOnly.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!DateOnly.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
