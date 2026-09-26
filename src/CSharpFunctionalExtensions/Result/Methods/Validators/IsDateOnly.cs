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
    public static Result<DateOnly> IsDateOnly(string? value, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .IsDateOnly(propertyNameFormat, arguments);
}
