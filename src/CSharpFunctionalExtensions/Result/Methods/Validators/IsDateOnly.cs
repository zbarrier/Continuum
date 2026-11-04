#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDateOnly(propertyName);

    /// <summary>
    ///     Ensure value is valid DateOnly.
    /// </summary>
    public static Result<DateOnly> IsDateOnly(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDateOnly(path, member);
}
