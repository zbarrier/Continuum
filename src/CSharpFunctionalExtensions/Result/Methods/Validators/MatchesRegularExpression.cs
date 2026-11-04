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
    public static Result<string> MatchesRegularExpression(string? value, string pattern, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .MatchesRegularExpression(pattern, path, member);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, Regex regex, string propertyName)
    => Result.NotNullOrEmpty(value, propertyName)
        .MatchesRegularExpression(regex, propertyName);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(string? value, Regex regex, PropertyPath path, string member)
    => Result.NotNullOrEmpty(value, path, member)
        .MatchesRegularExpression(regex, path, member);
}
