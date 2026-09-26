#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        return BigInteger.TryParse(result.Value, out BigInteger bigIntegerValue)
            ? Result.Success(bigIntegerValue)
            : Result.Failure<BigInteger>(new ValidationError(propertyName, RegularExpressionError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    public static Result<BigInteger> IsBigInteger(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return Result.Failure<BigInteger>(result.Error);

        if (!BigInteger.TryParse(result.Value, out BigInteger bigIntegerValue))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<BigInteger>(new ValidationError(propertyName, RegularExpressionError, propertyName));
        }

        return Result.Success(bigIntegerValue);
    }
}
