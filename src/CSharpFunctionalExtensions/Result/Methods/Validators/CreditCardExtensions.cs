// Portions of this file are adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: validation logic wrapped in Continuum Result/ValidationError types.

#nullable enable

namespace Continuum.CSharpFunctionalExtensions;

public static partial class ResultExtensions
{

    /// <summary>
    ///     Ensure value is valid credit card number.
    /// </summary>
    public static Result<string> IsCreditCard(this Result<string> result, string propertyName)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        int checksum = 0;
        bool evenDigit = false;
        // http://www.beachnet.com/~hstiles/cardtype.html
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char digit = value[i];

            if (!char.IsDigit(digit))
            {
                return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.CreditCard, ValidatorErrorStrings.CreditCard, propertyName));
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
            ? result
            : Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.CreditCard, ValidatorErrorStrings.CreditCard, propertyName));
    }

    /// <summary>
    ///     Ensure value is valid credit card number.
    /// </summary>
    public static Result<string> IsCreditCard(this Result<string> result, string propertyNameFormat, params object[] arguments)
    {
        if (result.IsFailure) return result;

        string value = result.Value;

        int checksum = 0;
        bool evenDigit = false;
        // http://www.beachnet.com/~hstiles/cardtype.html
        for (int i = value.Length - 1; i >= 0; i--)
        {
            char digit = value[i];

            if (!char.IsDigit(digit))
            {
                var propertyName = string.Format(propertyNameFormat, arguments);
                return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.CreditCard, ValidatorErrorStrings.CreditCard, propertyName));
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
            return Result.Failure<string>(new ValidationError(propertyName, ValidationErrorCodes.CreditCard, ValidatorErrorStrings.CreditCard, propertyName));
        }

        return result;
    }
}
