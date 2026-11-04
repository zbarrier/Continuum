#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    public static Result<int> IsInt32(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsInt32(propertyName);

    /// <summary>
    ///     Ensure value is valid int.
    /// </summary>
    public static Result<int> IsInt32(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsInt32(path, member);
}
