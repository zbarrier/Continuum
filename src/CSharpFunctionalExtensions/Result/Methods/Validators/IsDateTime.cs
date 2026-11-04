#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDateTime(propertyName);

    /// <summary>
    ///     Ensure value is valid DateTime.
    /// </summary>
    public static Result<DateTime> IsDateTime(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsDateTime(path, member);
}
