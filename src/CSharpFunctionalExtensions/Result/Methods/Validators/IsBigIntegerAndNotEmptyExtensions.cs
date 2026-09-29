#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is a valid BigInteger and not equal to BigInteger.Zero.
    /// </summary>
    public static Result<BigInteger> IsBigIntegerAndNotEmpty(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        return BigInteger.TryParse(result.Value, out BigInteger bigIntegerValue) && bigIntegerValue != BigInteger.Zero
            ? Result.Success(bigIntegerValue)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value is a valid BigInteger and not equal to BigInteger.Zero.
    /// </summary>
    public static Result<BigInteger> IsBigIntegerAndNotEmpty(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!BigInteger.TryParse(result.Value, out BigInteger bigIntegerValue) || bigIntegerValue == BigInteger.Zero)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return Result.Success(bigIntegerValue);
    }
}
