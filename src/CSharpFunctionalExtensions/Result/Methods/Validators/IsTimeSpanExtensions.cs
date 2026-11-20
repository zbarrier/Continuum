#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, string propertyName)
        => result.IsTimeSpan(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        return TimeSpan.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out TimeSpan parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, PropertyPath path, string member)
        => result.IsTimeSpan(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<TimeSpan> IsTimeSpan(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<TimeSpan>(result.Error);

        if (!TimeSpan.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out TimeSpan parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<TimeSpan>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
