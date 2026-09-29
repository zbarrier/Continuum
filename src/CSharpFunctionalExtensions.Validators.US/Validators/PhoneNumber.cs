#nullable enable

using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     US phone number validators exposed as static members of <see cref="Result"/>.
/// </summary>
public static partial class USPhoneNumberValidators
{
    [GeneratedRegex(@"\D+")]
    private static partial Regex NonPhoneNumberCharsRegex { get; }

    const int PhoneNumberLength = 10; //3365551234
    const int PhoneNumberExtensionMaxLength = 5; //12345

    extension(Result)
    {
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
            }

            // Check characters
            for (int i = 0; i < strippedValue.Length; i++)
            {
                bool isValidChar = char.IsDigit(strippedValue[i]);
                if (!isValidChar)
                {
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
            }

            // Check characters
            for (int i = 0; i < strippedValue.Length; i++)
            {
                bool isValidChar = char.IsDigit(strippedValue[i]);
                if (!isValidChar)
                {
                    var propertyName = string.Format(propertyNameFormat, arguments);
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
                }
            }

            return Result.Success(strippedValue);
        }

        /// <summary>
        ///     Ensure value is a valid US phone number extension (digits only, within the maximum length).
        /// </summary>
        public static Result<string> IsUSPhoneNumberExtension(string value, string propertyName)
        {
            bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
            if (!isValidLength)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
            }

            // Check characters
            for (int i = 0; i < value.Length; i++)
            {
                bool isValidChar = char.IsDigit(value[i]);
                if (!isValidChar)
                {
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
                }
            }

            return Result.Success(value);
        }

        /// <inheritdoc cref="IsUSPhoneNumberExtension(string, string)"/>
        public static Result<string> IsUSPhoneNumberExtension(string value, string propertyNameFormat, params object[] arguments)
        {
            bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
            if (!isValidLength)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
            }

            // Check characters
            for (int i = 0; i < value.Length; i++)
            {
                bool isValidChar = char.IsDigit(value[i]);
                if (!isValidChar)
                {
                    var propertyName = string.Format(propertyNameFormat, arguments);
                    return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
                }
            }

            return Result.Success(value);
        }
    }
}
