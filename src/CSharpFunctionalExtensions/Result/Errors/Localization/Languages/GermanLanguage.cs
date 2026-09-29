#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class GermanLanguage
{
    public const string Culture = "de";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ist keine gültige E-Mail-Adresse.",
        ValidationErrorCodes.GreaterThanOrEqual => "Der Wert von '{0}' muss grösser oder gleich '{1}' sein.",
        ValidationErrorCodes.GreaterThan => "Der Wert von '{0}' muss grösser sein als '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Der Wert von '{0}' muss kleiner oder gleich '{1}' sein.",
        ValidationErrorCodes.LessThan => "Der Wert von '{0}' muss kleiner sein als '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' darf nicht leer sein.",
        ValidationErrorCodes.NotNull => "'{0}' darf kein Nullwert sein.",
        ValidationErrorCodes.InvalidFormat => "'{0}' weist ein ungültiges Format auf.",
        ValidationErrorCodes.Equal => "'{0}' muss gleich '{1}' sein.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' muss zwischen {1} und {2} sein (exklusiv). Es wurde {3} eingegeben.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' muss zwischen {1} und {2} sein. Es wurde {3} eingegeben.",
        ValidationErrorCodes.CreditCard => "'{0}' ist keine gültige Kreditkartennummer.",
        ValidationErrorCodes.ScalePrecision => "'{0}' darf insgesamt nicht mehr als {1} Ziffern enthalten, mit Berücksichtigung von {2} Dezimalstellen. Es wurden {3} Ziffern und {4} Dezimalstellen gefunden.",
        ValidationErrorCodes.LengthBetween => "Die Länge von '{0}' muss zwischen {1} und {2} Zeichen liegen.",
        ValidationErrorCodes.MinLength => "Die Länge von '{0}' muss größer oder gleich {1} sein.",
        ValidationErrorCodes.MaxLength => "Die Länge von '{0}' muss kleiner oder gleich {1} sein.",
        ValidationErrorCodes.ExactLength => "'{0}' muss genau {1} lang sein.",
        _ => null,
    };
}
