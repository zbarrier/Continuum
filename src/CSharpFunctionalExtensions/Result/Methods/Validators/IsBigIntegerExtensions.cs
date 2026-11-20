#nullable enable

using System.Globalization;
using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, string propertyName)
        => result.IsBigInteger(propertyName, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, string propertyName, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        return BigInteger.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out BigInteger bigIntegerValue)
            ? Result.Success(bigIntegerValue)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, PropertyPath path, string member)
        => result.IsBigInteger(path, member, CultureInfo.InvariantCulture);

    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, PropertyPath path, string member, IFormatProvider? provider)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!BigInteger.TryParse(result.Value, provider ?? CultureInfo.InvariantCulture, out BigInteger bigIntegerValue))
        {
            var propertyName = path.Format(member);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(bigIntegerValue);
    }
}
