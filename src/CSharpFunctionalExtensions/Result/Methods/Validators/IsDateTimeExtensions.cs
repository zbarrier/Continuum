#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateTime> IsDateTime(this Result<string> result, string propertyName)
        => result.IsDateTime(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateTime> IsDateTime(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        return DateTime.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateTime> IsDateTime(this Result<string> result, PropertyPath path, string member)
        => result.IsDateTime(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateTime> IsDateTime(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<DateTime>(result.Error);

        if (!DateTime.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<DateTime>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
