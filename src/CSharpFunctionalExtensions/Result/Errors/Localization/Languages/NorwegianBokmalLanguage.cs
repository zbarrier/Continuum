#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class NorwegianBokmalLanguage
{
    public const string Culture = "nb";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' er ikke en gyldig e-postadresse.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' skal være større enn eller lik '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' skal være større enn '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' skal være mindre enn eller lik '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' skal være mindre enn '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' kan ikke være tom.",
        ValidationErrorCodes.NotNull => "'{0}' kan ikke være tom.",
        ValidationErrorCodes.InvalidFormat => "'{0}' har ikke riktig format.",
        ValidationErrorCodes.Equal => "'{0}' skal være lik med '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' skal være mellom {1} og {2}. Du har tastet inn {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' skal være mellom {1} og {2} (eksklusiv). Du har tastet inn {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' er ikke et gyldig kredittkortnummer.",
        ValidationErrorCodes.ScalePrecision => "'{0}' kan ikke være mer enn {1} siffer totalt, med hensyn til {2} desimaler. {3} siffer og {4} desimaler ble funnet.",
        ValidationErrorCodes.LengthBetween => "'{0}' skal være mellom {1} og {2} tegn.",
        ValidationErrorCodes.MinLength => "'{0}' skal være større enn eller lik {1} tegn.",
        ValidationErrorCodes.MaxLength => "'{0}' skal være mindre enn eller lik {1} tegn.",
        ValidationErrorCodes.ExactLength => "'{0}' skal være {1} tegn langt.",
        _ => null,
    };
}
