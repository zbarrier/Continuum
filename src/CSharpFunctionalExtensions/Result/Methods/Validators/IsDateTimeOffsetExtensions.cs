#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        return DateTimeOffset.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!DateTimeOffset.TryParse(result.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
