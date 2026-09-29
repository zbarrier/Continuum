#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class HungarianLanguage
{
    public const string Culture = "hu";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nem érvényes email cím.",
        ValidationErrorCodes.GreaterThanOrEqual => "A(z) '{0}' nagyobb vagy egyenlő kell, hogy legyen, mint '{1}'.",
        ValidationErrorCodes.GreaterThan => "A(z) '{0}' nagyobb kell, hogy legyen, mint '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "A(z) '{0}' kisebb vagy egyenlő kell, hogy legyen, mint '{1}'.",
        ValidationErrorCodes.LessThan => "A(z) '{0}' kisebb kell, hogy legyen, mint '{1}'.",
        ValidationErrorCodes.NotEmpty => "A(z) '{0}' nem lehet üres.",
        ValidationErrorCodes.NotNull => "A(z) '{0}' nem lehet üres.",
        ValidationErrorCodes.InvalidFormat => "A(z) '{0}' nem a megfelelő formátumban van.",
        ValidationErrorCodes.Equal => "A(z) '{0}' egyenlő kell, hogy legyen ezzel: '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "A(z) '{0}' nem lehet kisebb, mint {1} és nem lehet nagyobb, mint {2}. Ön ezt adta: {3}.",
        ValidationErrorCodes.ExclusiveBetween => "A(z) '{0}' nagyobb, mint {1} és kisebb, mint {2} kell, hogy legyen. Ön ezt adta: {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nem érvényes bankkártyaszám.",
        ValidationErrorCodes.ScalePrecision => "A(z) '{0}' összesen nem lehet több {1} számjegynél, {2} tizedesjegy pontosság mellett. {3} számjegy és {4} tizedesjegy pontosság lett megadva.",
        ValidationErrorCodes.LengthBetween => "A(z) '{0}' {1} és {2} karakter között kell, hogy legyen.",
        ValidationErrorCodes.MinLength => "A(z) '{0}' hossza legalább {1} karakter kell, hogy legyen.",
        ValidationErrorCodes.MaxLength => "A(z) '{0}' hossza legfeljebb {1} karakter lehet csak.",
        ValidationErrorCodes.ExactLength => "A(z) '{0}' pontosan {1} karakter hosszú lehet csak.",
        _ => null,
    };
}
