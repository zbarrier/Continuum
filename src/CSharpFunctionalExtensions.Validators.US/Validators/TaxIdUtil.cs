namespace Continuum.CSharpFunctionalExtensions;

internal static class TaxIdUtil
{
    private const char HyphenChar = '-';

    private static string GetTaxIdFormatCode(string taxIdErrorFormat) => taxIdErrorFormat switch
    {
        USValidatorErrorStrings.EinFormat => USValidationErrorCodes.EinFormat,
        USValidatorErrorStrings.SsnFormat => USValidationErrorCodes.SsnFormat,
        USValidatorErrorStrings.ItinFormat => USValidationErrorCodes.ItinFormat,
        _ => USValidationErrorCodes.TaxIdFormat,
    };


    internal const int EIN_Length = 10;
    internal const int EIN_FirstHyphenIndex = 2;

    internal const int SSN_ITIN_Length = 11;
    internal const int SSN_ITIN_FirstHyphenIndex = 3;
    internal const int SSN_ITIN_SecondHyphenIndex = 6;


    internal static Result CheckEinHyphenPositions(string value, string taxIdErrorFormat, string propertyName)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == EIN_FirstHyphenIndex)
        {
            // There should only be one hyphen in an EIN, so the first and last hyphen should be the same.
            int lastHyphenIndex = value.LastIndexOf(HyphenChar);
            if (firstHyphenIndex != lastHyphenIndex)
            {
                return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }

    internal static Result CheckEinHyphenPositions(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == EIN_FirstHyphenIndex)
        {
            // There should only be one hyphen in an EIN, so the first and last hyphen should be the same.
            int lastHyphenIndex = value.LastIndexOf(HyphenChar);
            if (firstHyphenIndex != lastHyphenIndex)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }


    internal static Result CheckSsnAndItinHyphenPositions(string value, string taxIdErrorFormat, string propertyName)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == SSN_ITIN_FirstHyphenIndex)
        {
            int lastHyphenIndex = value.LastIndexOf(HyphenChar);
            if (lastHyphenIndex != SSN_ITIN_SecondHyphenIndex)
            {
                return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }

    internal static Result CheckSsnAndItinHyphenPositions(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == SSN_ITIN_FirstHyphenIndex)
        {
            int lastHyphenIndex = value.LastIndexOf(HyphenChar);
            if (lastHyphenIndex != SSN_ITIN_SecondHyphenIndex)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }


    internal static Result<string> CheckEinDigits(string value, string taxIdErrorFormat, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == EIN_FirstHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckEinDigits(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == EIN_FirstHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }


    internal static Result<string> CheckSsnOrItinDigits(string value, string taxIdErrorFormat, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckSsnOrItinDigits(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, GetTaxIdFormatCode(taxIdErrorFormat), taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }


    internal static Result<string> CheckSsnDigits(string value, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isItin = i == 0 && c == '9';
            if (isItin)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.Ssn, USValidatorErrorStrings.Ssn, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.SsnFormat, USValidatorErrorStrings.SsnFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckSsnDigits(string value, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isItin = i == 0 && c == '9';
            if (isItin)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.Ssn, USValidatorErrorStrings.Ssn, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                ? c == '-'
                : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.SsnFormat, USValidatorErrorStrings.SsnFormat, propertyName));
            }
        }

        return Result.Success(value);
    }


    internal static Result<string> CheckItinDigits(string value, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isSsn = i == 0 && c != '9';
            if (isSsn)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.Itin, USValidatorErrorStrings.Itin, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                    ? c == '-'
                    : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ItinFormat, USValidatorErrorStrings.ItinFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckItinDigits(string value, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isSsn = i == 0 && c != '9';
            if (isSsn)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.Itin, USValidatorErrorStrings.Itin, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHyphenIndex || i == SSN_ITIN_SecondHyphenIndex
                    ? c == '-'
                    : char.IsAsciiDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, USValidationErrorCodes.ItinFormat, USValidatorErrorStrings.ItinFormat, propertyName));
            }
        }

        return Result.Success(value);
    }
}
