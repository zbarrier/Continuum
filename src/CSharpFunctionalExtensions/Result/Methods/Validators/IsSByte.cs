#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsSByte(propertyName);

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsSByte(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsSByte(path, member);

    /// <summary>
    ///     Ensure value is valid sbyte.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<sbyte> IsSByte(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsSByte(path, member, provider);
}
