#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    public static Result<TimeOnly> IsTimeOnly(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsTimeOnly(propertyName);

    /// <summary>
    ///     Ensure value is valid TimeOnly.
    /// </summary>
    public static Result<TimeOnly> IsTimeOnly(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsTimeOnly(path, member);
}
