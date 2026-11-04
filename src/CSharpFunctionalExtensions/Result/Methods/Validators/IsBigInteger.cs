#nullable enable

using System.Numerics;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    public static Result<BigInteger> IsBigInteger(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsBigInteger(propertyName);

    /// <summary>
    ///     Ensure value is valid BigInteger.
    /// </summary>
    public static Result<BigInteger> IsBigInteger(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsBigInteger(path, member);
}
