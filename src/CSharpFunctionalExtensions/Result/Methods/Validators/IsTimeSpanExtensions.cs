#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        return TimeSpan.TryParse(result.Value, CultureInfo.InvariantCulture, out TimeSpan parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!TimeSpan.TryParse(result.Value, CultureInfo.InvariantCulture, out TimeSpan parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
