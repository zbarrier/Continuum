#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{
    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        var maxLengthResult = Result.MaxLength(result.Value, EmailValidation.MaxEmailLength, propertyName);
        if (maxLengthResult.IsFailure) return maxLengthResult;

        return EmailValidation.EmailRegex.IsMatch(result.Value)
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        var maxLengthResult = Result.MaxLength(result.Value, EmailValidation.MaxEmailLength, propertyNameFormat, arguments);
        if (maxLengthResult.IsFailure) return maxLengthResult;

        if (EmailValidation.EmailRegex.IsMatch(result.Value)) return result;
            
        var propertyName = string.Format(propertyNameFormat, arguments);
        return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
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
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
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
        return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }
}
