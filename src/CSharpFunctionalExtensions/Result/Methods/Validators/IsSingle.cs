#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<float> IsSingle(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsSingle(propertyName);

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<float> IsSingle(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsSingle(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<float> IsSingle(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsSingle(path, member);

    /// <summary>
    ///     Ensure value is valid, finite float. NaN, infinity and values that overflow to infinity are rejected.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<float> IsSingle(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsSingle(path, member, provider);
}
