#nullable enable

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public partial class ResultExtensions
{
    [GeneratedRegex(@"\D+")]
    private static partial Regex NonPhoneNumberCharsRegex { get; }

    const string PhoneNumberSimpleError = "'{0}' is not a valid US phone number.";
    const string PhoneNumberExtensionSimpleError = "'{0}' is not a valid US phone number extension.";

    const int PhoneNumberLength = 10; //3365551234
    const int PhoneNumberExtensionMaxLength = 5; //12345

    /// <summary>
    ///     Ensure value is valid US phone number.
    /// </summary>
    public static Result<string> IsUSPhoneNumber(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        // Strip unnecessary characters
        string strippedValue = value.Length <= PhoneNumberLength
            ? value
            : NonPhoneNumberCharsRegex.Replace(value, "");

        // Check length
        bool isValidLength = strippedValue.Length == PhoneNumberLength;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberSimpleError, propertyName));
        }

        // Check characters
        for (int i = 0; i < strippedValue.Length; i++)
        {
            bool isValidChar = char.IsDigit(strippedValue[i]);
            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
            }
        }

        return Result.Success(strippedValue);
    }

    /// <summary>
    ///     Ensure value is valid US phone number.
    /// </summary>
    public static Result<string> IsUSPhoneNumber(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        // Strip unnecessary characters
        string strippedValue = value.Length <= PhoneNumberLength
            ? value
            : NonPhoneNumberCharsRegex.Replace(value, "");

        // Check length
        bool isValidLength = strippedValue.Length == PhoneNumberLength;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberSimpleError, propertyName));
        }

        // Check characters
        for (int i = 0; i < strippedValue.Length; i++)
        {
            bool isValidChar = char.IsDigit(strippedValue[i]);
            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, ZipCodeSimpleError, propertyName));
            }
        }

        return Result.Success(strippedValue);
    }

    public static Result<string> IsUSPhoneNumberExtension(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberExtensionSimpleError, propertyName));
        }

        // Check characters
        for (int i = 0; i < value.Length; i++)
        {
            bool isValidChar = char.IsDigit(value[i]);
            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberExtensionSimpleError, propertyName));
            }
        }

        return Result.Success(value);
    }

    public static Result<string> IsUSPhoneNumberExtension(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
        if (!isValidLength)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberExtensionSimpleError, propertyName));
        }

        // Check characters
        for (int i = 0; i < value.Length; i++)
        {
            bool isValidChar = char.IsDigit(value[i]);
            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberExtensionSimpleError, propertyName));
            }
        }

        return Result.Success(value);
    }
}
