#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    public static Result<TimeSpan> IsTimeSpan(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsTimeSpan(propertyName);

    /// <summary>
    ///     Ensure value is valid TimeSpan.
    /// </summary>
    public static Result<TimeSpan> IsTimeSpan(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsTimeSpan(path, member);
}
