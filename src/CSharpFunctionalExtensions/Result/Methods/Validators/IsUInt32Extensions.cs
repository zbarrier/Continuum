#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid uint.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<uint> IsUInt32(this Result<string> result, string propertyName)
        => result.IsUInt32(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid uint.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<uint> IsUInt32(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<uint>(result.Error);

        return uint.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out uint parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid uint.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<uint> IsUInt32(this Result<string> result, PropertyPath path, string member)
        => result.IsUInt32(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid uint.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<uint> IsUInt32(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<uint>(result.Error);

        if (!uint.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out uint parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<uint>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
