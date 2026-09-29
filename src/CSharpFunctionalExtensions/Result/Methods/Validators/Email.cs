#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(string value, string propertyName)
    {
        var result = Result.MaxLength(value, EmailValidation.MaxEmailLength, propertyName);
        if (result.IsFailure)
        {
            return result;
        }

        return EmailValidation.EmailRegex.IsMatch(value)
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid email.
    /// </summary>
    public static Result<string> IsEmail(string value, string propertyNameFormat, params object[] arguments)
    {
        var result = Result.MaxLength(value, EmailValidation.MaxEmailLength, propertyNameFormat, arguments);
        if (result.IsFailure)
        {
            return result;
        }
        if (EmailValidation.EmailRegex.IsMatch(value))
        {
            return Result.Success(value);
        }
            
        var propertyName = string.Format(propertyNameFormat, arguments);
        return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }

    /// <summary>
    ///     Ensure value is ASP.NET Core compatible email.
    /// </summary>
    public static Result<string> IsAspNetCoreCompatibleEmail(string value, string propertyName)
    {
        int index = value.IndexOf('@');

        return index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@')
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }

    /// <summary>
    ///     Ensure value is ASP.NET Core compatible email.
    /// </summary>
    public static Result<string> IsAspNetCoreCompatibleEmail(string value, string propertyNameFormat, params object[] arguments)
    {
        int index = value.IndexOf('@');

        if (index > 0 && index != value.Length - 1 && index == value.LastIndexOf('@'))
            return Result.Success(value);

        var propertyName = string.Format(propertyNameFormat, arguments);
        return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.Email, ValidatorErrorStrings.Email, propertyName));
    }
}
