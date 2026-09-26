#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    public static Result<decimal> IsDecimal(string? value, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .IsDecimal(propertyName);

    /// <summary>
    ///     Ensure value is valid decimal.
    /// </summary>
    public static Result<decimal> IsDecimal(string? value, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .IsDecimal(propertyNameFormat, arguments);
}
