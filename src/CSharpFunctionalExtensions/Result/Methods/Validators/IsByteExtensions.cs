#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid byte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<byte> IsByte(this Result<string> result, string propertyName)
        => result.IsByte(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid byte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<byte> IsByte(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        return byte.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out byte parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid byte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<byte> IsByte(this Result<string> result, PropertyPath path, string member)
        => result.IsByte(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid byte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<byte> IsByte(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<byte>(result.Error);

        if (!byte.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out byte parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<byte>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
