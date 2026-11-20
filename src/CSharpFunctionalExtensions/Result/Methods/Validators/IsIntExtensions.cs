#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<int> IsInt32(this Result<string> result, string propertyName)
        => result.IsInt32(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<int> IsInt32(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        return int.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out int intValue)
            ? Result.Success(intValue)
            : Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<int> IsInt32(this Result<string> result, PropertyPath path, string member)
        => result.IsInt32(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<int> IsInt32(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<int>(result.Error);

        if (!int.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out int intValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<int>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(intValue);
    }
}
