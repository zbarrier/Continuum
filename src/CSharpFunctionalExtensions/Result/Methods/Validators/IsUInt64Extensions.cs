#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid ulong.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<ulong> IsUInt64(this Result<string> result, string propertyName)
        => result.IsUInt64(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid ulong.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<ulong> IsUInt64(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<ulong>(result.Error);

        return ulong.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out ulong parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid ulong.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<ulong> IsUInt64(this Result<string> result, PropertyPath path, string member)
        => result.IsUInt64(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid ulong.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<ulong> IsUInt64(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<ulong>(result.Error);

        if (!ulong.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out ulong parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<ulong>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
