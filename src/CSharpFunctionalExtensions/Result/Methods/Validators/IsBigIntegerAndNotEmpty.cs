#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is a valid BigInteger and not equal to BigInteger.Zero.
    /// </summary>
    public static Result<BigInteger> IsBigIntegerAndNotEmpty(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsBigIntegerAndNotEmpty(propertyName);

    /// <summary>
    ///     Ensure value is valid BigInteger and not equal to BigInteger.Zero.
    /// </summary>
    public static Result<BigInteger> IsBigIntegerAndNotEmpty(string? value, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .IsBigIntegerAndNotEmpty(propertyNameFormat, arguments);
}
