#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDateOnly(propertyName);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(string? value, string propertyName, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDateOnly(propertyName, provider);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDateOnly(path, member);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    /// <remarks>Parses with <paramref name="provider"/>, such as the user's culture for localized form input. Null uses the invariant culture.</remarks>
    public static Result<DateOnly> IsDateOnly(string? value, PropertyPath path, string member, IFormatProvider? provider)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDateOnly(path, member, provider);
}
