#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDecimal(propertyName);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDecimal(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDecimal(path, member);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<decimal> IsDecimal(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDecimal(path, member, provider);
}
