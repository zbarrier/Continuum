#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<short> IsInt16(this Result<string> result, string propertyName)
        => result.IsInt16(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<short> IsInt16(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        return short.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out short parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<short> IsInt16(this Result<string> result, PropertyPath path, string member)
        => result.IsInt16(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<short> IsInt16(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<short>(result.Error);

        if (!short.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out short parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<short>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
