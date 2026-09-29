#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, string pattern, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .MatchesRegularExpression(pattern, propertyName);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, string pattern, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .MatchesRegularExpression(pattern, propertyNameFormat, arguments);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, Regex regex, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .MatchesRegularExpression(regex, propertyName);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, Regex regex, string propertyNameFormat, params object[] arguments)
    => Result.NotNullOrEmpty(value, propertyNameFormat, arguments)
        .MatchesRegularExpression(regex, propertyNameFormat, arguments);
}
