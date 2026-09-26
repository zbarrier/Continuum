#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is a valid Guid and not equal to Guid.Empty.
    /// </summary>
    public static Result<Guid> IsGuidAndNotEmpty(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsGuidAndNotEmpty(propertyName);

    /// <summary>
    ///     Ensure value is valid Guid.
    /// </summary>
    public static Result<Guid> IsGuidAndNotEmpty(string? value, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .IsGuidAndNotEmpty(propertyNameFormat, arguments);
}
