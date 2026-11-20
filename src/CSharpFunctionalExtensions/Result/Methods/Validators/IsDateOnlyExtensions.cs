#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, string propertyName)
        => result.IsDateOnly(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        return DateOnly.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, PropertyPath path, string member)
        => result.IsDateOnly(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateOnly>(result.Error);

        if (!DateOnly.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
