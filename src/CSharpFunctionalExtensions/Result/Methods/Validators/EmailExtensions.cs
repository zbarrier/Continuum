#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    const int MaxEmailLength = 254;

    const string EmailError = "'{0}' is not a valid email address.";
    const string EmailExpression = "^((([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+(\\.([a-z]|\\d|[!#\\$%&'\\*\\+\\-\\/=\\?\\^_`{\\|}~]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+)*)|((\\x22)((((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(([\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x7f]|\\x21|[\\x23-\\x5b]|[\\x5d-\\x7e]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(\\\\([\\x01-\\x09\\x0b\\x0c\\x0d-\\x7f]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF]))))*(((\\x20|\\x09)*(\\x0d\\x0a))?(\\x20|\\x09)+)?(\\x22)))@((([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])|(([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])([a-z]|\\d|-||_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])*([a-z]|\\d|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))\\.)+(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+|(([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])+([a-z]+|\\d|-|\\.{0,1}|_|~|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])?([a-z]|[\\u00A0-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFEF])))$";
    const RegexOptions EmailRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;

    static readonly Regex EmailRegex = new Regex(EmailExpression, EmailRegexOptions, TimeSpan.FromSeconds(2.0));

    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        var maxLengthResult = Result.MaxLength(result.Value, MaxEmailLength, propertyName);
        if (maxLengthResult.IsFailure) return maxLengthResult;

        return EmailRegex.IsMatch(result.Value)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, EmailError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var maxLengthResult = Result.MaxLength(result.Value, MaxEmailLength, propertyNameFormat, arguments);
        if (maxLengthResult.IsFailure) return maxLengthResult;

        if (EmailRegex.IsMatch(result.Value)) return result;
            
        var propertyName = string.Format(propertyNameFormat, arguments);
        return Result.Failure<string>(new ValidationError(propertyName, EmailError, propertyName));
    }

    /// <summary>
    ///     Ensure value is ASP.NET Core compatible email.
    /// </summary>
    public static Result<string> IsAspNetCoreCompatibleEmail(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        int index = value.IndexOf('@');

        return index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@')
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, EmailError, propertyName));
    }

    /// <summary>
    ///     Ensure value is ASP.NET Core compatible email.
    /// </summary>
    public static Result<string> IsAspNetCoreCompatibleEmail(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        int index = value.IndexOf('@');

        if (index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@')) return result;

        var propertyName = string.Format(propertyNameFormat, arguments);
        return Result.Failure<string>(new ValidationError(propertyName, EmailError, propertyName));
    }
}
