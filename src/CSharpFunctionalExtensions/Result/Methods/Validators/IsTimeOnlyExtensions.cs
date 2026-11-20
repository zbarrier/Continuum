#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, string propertyName)
        => result.IsTimeOnly(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        return TimeOnly.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, PropertyPath path, string member)
        => result.IsTimeOnly(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<TimeOnly> IsTimeOnly(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<TimeOnly>(result.Error);

        if (!TimeOnly.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeOnly>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
