#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    public static Result<long> IsInt64(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsInt64(propertyName);

    /// <summary>
    ///     Ensure value is valid long.
    /// </summary>
    public static Result<long> IsInt64(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsInt64(path, member);
}
