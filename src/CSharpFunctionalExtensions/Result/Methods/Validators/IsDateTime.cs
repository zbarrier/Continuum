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
    public static Result<DateTime> IsDateTime(string? value, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .IsDateTime(propertyNameFormat, arguments);
}
