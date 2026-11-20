#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<long> IsInt64(this Result<string> result, string propertyName)
        => result.IsInt64(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<long> IsInt64(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        return long.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out long longValue)
            ? Result.Success(longValue)
            : Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<long> IsInt64(this Result<string> result, PropertyPath path, string member)
        => result.IsInt64(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<long> IsInt64(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<long>(result.Error);

        if (!long.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out long longValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<long>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(longValue);
    }
}
