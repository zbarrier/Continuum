#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    public static Result<DateTimeOffset> IsDateTimeOffset(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDateTimeOffset(propertyName);

    /// <summary>
    ///     Ensure value is valid DateTimeOffset.
    /// </summary>
    public static Result<DateTimeOffset> IsDateTimeOffset(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDateTimeOffset(path, member);
}
