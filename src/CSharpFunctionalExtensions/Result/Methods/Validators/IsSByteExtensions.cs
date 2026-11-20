#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(this Result<string> result, string propertyName)
        => result.IsSByte(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<sbyte>(result.Error);

        return sbyte.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out sbyte parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<sbyte>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(this Result<string> result, PropertyPath path, string member)
        => result.IsSByte(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<sbyte>(result.Error);

        if (!sbyte.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out sbyte parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<sbyte>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
