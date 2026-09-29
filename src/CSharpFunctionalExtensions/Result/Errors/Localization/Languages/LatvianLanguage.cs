#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class LatvianLanguage
{
    public const string Culture = "lv";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nesatur pareizu e-pasta adresi.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' vērtībai ir jābūt lielākai vai vienādai ar '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' vērtībai ir jābūt lielākai par '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' vērtībai ir jābūt mazākai vai vienādai ar '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' vērtībai ir jābūt mazākai par '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' vērtība nevar būt tukša.",
        ValidationErrorCodes.NotNull => "'{0}' vērtība nevar būt tukša.",
        ValidationErrorCodes.InvalidFormat => "'{0}' nav ievadīts vajadzīgajā formātā.",
        ValidationErrorCodes.Equal => "'{0}' vērtībai ir jābūt vienādai ar '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' vērtībai ir jābūt no {1} līdz {2}. Ievadītā vērtība: {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' vērtībai ir jābūt no {1} līdz {2} (neiekļaujot šīs vērtības). Ievadītā vērtība: {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nesatur pareizu kredītkartes numuru.",
        ValidationErrorCodes.ScalePrecision => "'{0}' vērtība nedrīkst saturēt vairāk par {1} ciparu kopā, tajā skaitā {2} ciparu aiz komata. Ievadītā vērtība satur {3} ciparu kopā un {4} ciparu aiz komata.",
        ValidationErrorCodes.LengthBetween => "'{0}' vērtībai ir jābūt no {1} līdz {2} simbolu garai.",
        ValidationErrorCodes.MinLength => "'{0}' vērtībai ir jābūt vismaz {1} simbolu garai.",
        ValidationErrorCodes.MaxLength => "'{0}' vērtībai ir jābūt maksimums {1} simbolu garai.",
        ValidationErrorCodes.ExactLength => "'{0}' vērtībai ir jābūt {1} simbolu garai.",
        _ => null,
    };
}
