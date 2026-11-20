#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<short> IsInt16(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsInt16(propertyName);

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<short> IsInt16(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsInt16(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<short> IsInt16(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsInt16(path, member);

    /// <summary>
    ///     Ensure value is valid short.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<short> IsInt16(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsInt16(path, member, provider);
}
