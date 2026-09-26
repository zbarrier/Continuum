#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
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
    public static Result<string> IsUSPhoneNumber(string value, string propertyName)
    {
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
                return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberSimpleError, propertyName));
            }
        }

        return Result.Success(strippedValue);
    }

    /// <summary>
    ///     Ensure value is valid US phone number.
    /// </summary>
    public static Result<string> IsUSPhoneNumber(string value, string propertyNameFormat, params object[] arguments)
    {
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
                return Result.Failure<string>(new ValidationError(propertyName, PhoneNumberSimpleError, propertyName));
            }
        }

        return Result.Success(strippedValue);
    }

    public static Result<string> IsUSPhoneNumberExtension(string value, string propertyName)
    {
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

    public static Result<string> IsUSPhoneNumberExtension(string value, string propertyNameFormat, params object[] arguments)
    {
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
