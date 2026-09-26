#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public partial struct Result
{
    const string CreditCardError = "'{0}' is not a valid credit card number.";

    /// <summary>
    ///     Ensure value is valid credit card number.
    /// </summary>
    public static Result<string> IsCreditCard(string value, string propertyName)
    {
        int checksum = 0;
        bool evenDigit = false;
        // http://www.beachnet.com/~hstiles/cardtype.html
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char digit = value[i];

            if (!char.IsDigit(digit))
            {
                return Result.Failure<string>(new ValidationError(propertyName, CreditCardError, propertyName));
            }
                
            int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
            evenDigit = !evenDigit;

            while (digitValue > 0)
            {
                checksum += digitValue % 10;
                digitValue /= 10;
            }
        }

        return (checksum % 10) == 0
            ? Result.Success(value)
            : Result.Failure<string>(new ValidationError(propertyName, CreditCardError, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid credit card number.
    /// </summary>
    public static Result<string> IsCreditCard(string value, string propertyNameFormat, params object[] arguments)
    {
        int checksum = 0;
        bool evenDigit = false;
        // http://www.beachnet.com/~hstiles/cardtype.html
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char digit = value[i];

            if (!char.IsDigit(digit))
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, CreditCardError, propertyName));
            }

            int digitValue = (digit - '0') * (evenDigit ? 2 : 1);
            evenDigit = !evenDigit;

            while (digitValue > 0)
            {
                checksum += digitValue % 10;
                digitValue /= 10;
            }
        }

        if (checksum % 10 != 0)
        {
            var propertyName = string.Format(propertyNameFormat, arguments);
            return Result.Failure<string>(new ValidationError(propertyName, CreditCardError, propertyName));
        }

        return Result.Success(value);
    }
}
