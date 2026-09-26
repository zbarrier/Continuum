namespace Continuum.CSharpFunctionalExtensions;

internal static class TaxIdUtil
{
    private const char HyphenChar = '-';

    internal const string SimpleError = "'{0}' is not a valid US Tax ID.";
    internal const string FormatError = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format and a SSN or ITIN should use the '123-12-1234' format.";
    internal const string EinFormatError = "'{0}' is not formatted correctly. An EIN should use the '12-1234567' format.";
    internal const string SsnFormatError = "'{0}' is not formatted correctly. A SSN should use the '123-12-1234' format.";
    internal const string ItinFormatError = "'{0}' is not formatted correctly. An ITIN should use the '123-12-1234' format.";
    internal const string IsNotSsnError = "'{0}' is not a valid SSN. An SSN must start with a digit other than '9'. An ITIN must start with '9'.";
    internal const string IsNotItinError = "'{0}' is not a valid ITIN. An ITIN must start with '9'. An SSN must start with a digit other than '9'.";

    internal const int EIN_Length = 10;
    internal const int EIN_FirstHypenIndex = 2;

    internal const int SSN_ITIN_Length = 11;
    internal const int SSN_ITIN_FirstHypenIndex = 3;
    internal const int SSN_ITIN_SecondHypenIndex = 6;


    internal static Result CheckEinHyphenPositions(string value, string taxIdErrorFormat, string propertyName)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == EIN_FirstHypenIndex)
        {
            // There should only be one hyphen in an EIN, so the first and last hyphen should be the same.
            int lastHypenIndex = value.LastIndexOf(HyphenChar);
            if (firstHyphenIndex != lastHypenIndex)
            {
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }

    internal static Result CheckEinHyphenPositions(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == EIN_FirstHypenIndex)
        {
            // There should only be one hyphen in an EIN, so the first and last hyphen should be the same.
            int lastHypenIndex = value.LastIndexOf(HyphenChar);
            if (firstHyphenIndex != lastHypenIndex)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }


    internal static Result CheckSsnAndItinHyphenPositions(string value, string taxIdErrorFormat, string propertyName)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == SSN_ITIN_FirstHypenIndex)
        {
            int lastHypenIndex = value.LastIndexOf(HyphenChar);
            if (lastHypenIndex != SSN_ITIN_SecondHypenIndex)
            {
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }

    internal static Result CheckSsnAndItinHyphenPositions(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        int firstHyphenIndex = value.IndexOf(HyphenChar);
        if (firstHyphenIndex == SSN_ITIN_FirstHypenIndex)
        {
            int lastHypenIndex = value.LastIndexOf(HyphenChar);
            if (lastHypenIndex != SSN_ITIN_SecondHypenIndex)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }
        else
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
        }

        return Result.Success();
    }


    internal static Result<string> CheckEinDigits(string value, string taxIdErrorFormat, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == EIN_FirstHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckEinDigits(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == EIN_FirstHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }


    internal static Result<string> CheckSsnOrItinDigits(string value, string taxIdErrorFormat, string propertyName)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
            }
        }

        return Result.Success(value);
    }

    internal static Result<string> CheckSsnOrItinDigits(string value, string taxIdErrorFormat, string propertyNameFormat, params object[] arguments)
    {
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, taxIdErrorFormat, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, IsNotSsnError, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, SsnFormatError, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, IsNotSsnError, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                ? c == '-'
                : char.IsDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, SsnFormatError, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, IsNotItinError, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                    ? c == '-'
                    : char.IsDigit(c);

            if (!isValidChar)
            {
                return Result.Failure<string>(new ValidationError(propertyName, ItinFormatError, propertyName));
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
                return Result.Failure<string>(new ValidationError(propertyName, IsNotItinError, propertyName));
            }

            bool isValidChar = i == SSN_ITIN_FirstHypenIndex || i == SSN_ITIN_SecondHypenIndex
                    ? c == '-'
                    : char.IsDigit(c);

            if (!isValidChar)
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, ItinFormatError, propertyName));
            }
        }

        return Result.Success(value);
    }
}
