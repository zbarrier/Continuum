#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class DutchLanguage
{
    public const string Culture = "nl";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' is geen geldig email adres.",
        ValidationErrorCodes.Equal => "'{0}' moet gelijk zijn aan '{1}'.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' moet groter zijn dan of gelijk zijn aan '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' moet groter zijn dan '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' moet kleiner zijn dan of gelijk zijn aan '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' moet kleiner zijn dan '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' mag niet leeg zijn.",
        ValidationErrorCodes.NotNull => "'{0}' mag niet leeg zijn.",
        ValidationErrorCodes.InvalidFormat => "'{0}' voldoet niet aan het verwachte formaat.",
        ValidationErrorCodes.CreditCard => "'{0}' is geen geldig credit card nummer.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' moet na {1} komen en voor {2} liggen. U heeft '{3}' ingevuld.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' moet tussen {1} en {2} liggen. U heeft '{3}' ingevuld.",
        ValidationErrorCodes.ScalePrecision => "'{0}' mag in totaal niet meer dan {1} decimalen nauwkeurig zijn, met een grootte van {2} gehele getallen. Er zijn {3} decimalen en een grootte van {4} gehele getallen gevonden.",
        ValidationErrorCodes.LengthBetween => "De lengte van '{0}' moet tussen {1} en {2} karakters zijn.",
        ValidationErrorCodes.MinLength => "De lengte van '{0}' moet groter zijn dan of gelijk zijn aan {1} karakters.",
        ValidationErrorCodes.MaxLength => "De lengte van '{0}' moet kleiner zijn dan of gelijk zijn aan {1} karakters.",
        ValidationErrorCodes.ExactLength => "De lengte van '{0}' moet {1} karakters zijn.",
        _ => null,
    };
}
