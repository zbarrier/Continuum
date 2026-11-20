#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<double> IsDouble(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDouble(propertyName);

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<double> IsDouble(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDouble(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<double> IsDouble(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDouble(path, member);

    /// <summary>
    ///     Ensure value is valid, finite double. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<double> IsDouble(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDouble(path, member, provider);
}
