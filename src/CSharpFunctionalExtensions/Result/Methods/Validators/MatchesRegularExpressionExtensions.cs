#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    static readonly TimeSpan RegexMatchTimeout = TimeSpan.FromSeconds(2.0);

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, string pattern, string propertyName)
    {
        if (result.IsFailure) return result;

        return Regex.IsMatch(result.Value, pattern, RegexOptions.None, RegexMatchTimeout)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, string pattern, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (!Regex.IsMatch(result.Value, pattern, RegexOptions.None, RegexMatchTimeout))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return result;
    }

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, Regex regex, string propertyName)
    {
        if (result.IsFailure) return result;

        return regex.IsMatch(result.Value)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
    }

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, Regex regex, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        if (!regex.IsMatch(result.Value))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.InvalidFormat, ValidatorErrorStrings.InvalidFormat, propertyName));
        }

        return result;
    }
}
