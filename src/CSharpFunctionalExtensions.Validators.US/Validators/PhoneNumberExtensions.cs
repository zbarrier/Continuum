#nullable enable

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     US phone number validators for chaining on <see cref="Result{T}"/>.
/// </summary>
public static partial class USPhoneNumberValidatorExtensions
{
    [GeneratedRegex(@"\D+")]
    private static partial Regex NonPhoneNumberCharsRegex { get; }

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
            return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
        }

        // Check characters
        for (int i = 0; i < strippedValue.Length; i++)
        {
            bool isValidChar = char.IsAsciiDigit(strippedValue[i]);
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
    public static Result<string> IsUSPhoneNumber(this Result<string> result, PropertyPath path, string member)
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
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
        }

        // Check characters
        for (int i = 0; i < strippedValue.Length; i++)
        {
            bool isValidChar = char.IsAsciiDigit(strippedValue[i]);
            if (!isValidChar)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumber, USValidatorErrorStrings.PhoneNumber, propertyName));
            }
        }

        return Result.Success(strippedValue);
    }

    /// <summary>
    ///     Ensure value is a valid US phone number extension (digits only, within the maximum length).
    /// </summary>
    public static Result<string> IsUSPhoneNumberExtension(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
        if (!isValidLength)
        {
            return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
        }

        // Check characters
        for (int i = 0; i < value.Length; i++)
        {
            bool isValidChar = char.IsAsciiDigit(value[i]);
            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
            }
        }

        return Result.Success(value);
    }

    /// <inheritdoc cref="IsUSPhoneNumberExtension(Result{string}, string)"/>
    public static Result<string> IsUSPhoneNumberExtension(this Result<string> result, PropertyPath path, string member)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        bool isValidLength = value.Length <= PhoneNumberExtensionMaxLength;
        if (!isValidLength)
        {
            var propertyName = path.Format(member);
            return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
        }

        // Check characters
        for (int i = 0; i < value.Length; i++)
        {
            bool isValidChar = char.IsAsciiDigit(value[i]);
            if (!isValidChar)
            {
                var propertyName = path.Format(member);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.PhoneNumberExtension, USValidatorErrorStrings.PhoneNumberExtension, propertyName));
            }
        }

        return Result.Success(value);
    }
}
