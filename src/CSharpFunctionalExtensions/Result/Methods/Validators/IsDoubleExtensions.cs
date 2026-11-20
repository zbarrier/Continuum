#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<double> IsDouble(this Result<string> result, string propertyName)
        => result.IsDouble(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<double> IsDouble(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        return double.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out double parsedValue) && double.IsFinite(parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<double> IsDouble(this Result<string> result, PropertyPath path, string member)
        => result.IsDouble(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<double> IsDouble(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<double>(result.Error);

        if (!double.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out double parsedValue) || !double.IsFinite(parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<double>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
