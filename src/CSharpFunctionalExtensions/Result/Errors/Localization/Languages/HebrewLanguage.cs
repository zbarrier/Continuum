#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class HebrewLanguage
{
    public const string Culture = "he";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' אינה כתובת דוא\"ל חוקית.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' חייב להיות גדול או שווה ל- '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' חייב להיות גדול מ- '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' חייב להיות קטן או שווה ל- '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' חייב להיות קטן מ- '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' לא אמור להיות ריק.",
        ValidationErrorCodes.NotNull => "'{0}' לא יכול להיות ריק.",
        ValidationErrorCodes.InvalidFormat => "'{0}' אינו בפורמט הנכון.",
        ValidationErrorCodes.Equal => "'{0}' אמור להיות שווה ל- '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' חייב להיות בין {1} לבין {2}. הזנת {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' חייב להיות בין {1} לבין {2} (לא כולל). הזנת {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' אינו מספר כרטיס אשראי חוקי.",
        ValidationErrorCodes.ScalePrecision => "'{0}' לא יכול לכלול יותר מ- {1} ספרות בסך הכל, עם הקצבה של {2} ספרות עשרוניות. נמצאו {3} ספרות ו- {4} ספרות עשרוניות.",
        ValidationErrorCodes.LengthBetween => "אורך '{0}' חייב להיות בין {1} ל- {2}.",
        ValidationErrorCodes.MinLength => "אורך '{0}' חייב להיות לפחות {1} תווים.",
        ValidationErrorCodes.MaxLength => "אורך '{0}' חייב להיות {1} תווים או פחות.",
        ValidationErrorCodes.ExactLength => "'{0}' חייב להיות באורך {1} תווים.",
        _ => null,
    };
}
