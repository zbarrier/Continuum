#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, string propertyName)
        => result.IsDateTimeOffset(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        return DateTimeOffset.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, PropertyPath path, string member)
        => result.IsDateTimeOffset(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateTimeOffset> IsDateTimeOffset(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateTimeOffset>(result.Error);

        if (!DateTimeOffset.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTimeOffset>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
