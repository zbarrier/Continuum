#nullable enable

using System.Globalization;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<float> IsSingle(this Result<string> result, string propertyName)
        => result.IsSingle(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<float> IsSingle(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        return float.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out float parsedValue) && float.IsFinite(parsedValue)
            ? Result.Success(parsedValue)
            : Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<float> IsSingle(this Result<string> result, PropertyPath path, string member)
        => result.IsSingle(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<float> IsSingle(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<float>(result.Error);

        if (!float.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out float parsedValue) || !float.IsFinite(parsedValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<float>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(parsedValue);
    }
}
