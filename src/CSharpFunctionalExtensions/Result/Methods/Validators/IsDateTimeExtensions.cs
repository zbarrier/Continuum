#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        return DateTime.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!DateTime.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
