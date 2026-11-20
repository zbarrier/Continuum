#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(this Result<string> result, string propertyName)
        => result.IsDecimal(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        return decimal.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out decimal decimalValue)
            ? Result.Success(decimalValue)
            : Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(this Result<string> result, PropertyPath path, string member)
        => result.IsDecimal(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<decimal>(result.Error);

        if (!decimal.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out decimal decimalValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<decimal>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(decimalValue);
    }
}
