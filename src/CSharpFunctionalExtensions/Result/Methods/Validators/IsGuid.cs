#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuid(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsGuid(propertyName);

    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuid(string? value, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .IsGuid(path, member);
}
