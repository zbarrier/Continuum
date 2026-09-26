#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const string RegularExpressionError = "'{0}' is not in the correct format.";

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, string pattern, string propertyName)
    {
        if (result.IsFailure) return result;

        var regex = new Regex(pattern);

        return regex.IsMatch(result.Value)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, RegularExpressionError, propertyName));
    }

    /// <summary>
    ///     Ensure value matches regular expression.
    /// </summary>
    public static Result<string> MatchesRegularExpression(this Result<string> result, string pattern, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var regex = new Regex(pattern);

        if (!regex.IsMatch(result.Value))
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, RegularExpressionError, propertyName));
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
            : Result.Failure<string>(new ValidationError(propertyName, RegularExpressionError, propertyName));
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
            return Result.Failure<string>(new ValidationError(propertyName, RegularExpressionError, propertyName));
        }

        return result;
    }
}
