#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class ItalianLanguage
{
    public const string Culture = "it";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' non è un indirizzo email valido.",
        ValidationErrorCodes.Equal => "'{0}' dovrebbe essere uguale a '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' deve essere compreso tra {1} e {2} (esclusi). Hai inserito {3}.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' deve essere maggiore o uguale a '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' deve essere maggiore di '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' deve essere compreso tra {1} e {2}. Hai inserito {3}.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' deve essere minore o uguale a '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' deve essere minore di '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' non può essere vuoto.",
        ValidationErrorCodes.NotNull => "'{0}' non può essere vuoto.",
        ValidationErrorCodes.InvalidFormat => "'{0}' non è nel formato corretto.",
        ValidationErrorCodes.CreditCard => "'{0}' non è un numero di carta di credito valido.",
        ValidationErrorCodes.ScalePrecision => "'{0}' non può avere più di {1} cifre in totale, con una tolleranza per {2} decimali. Sono state trovate {3} cifre e {4} decimali.",
        ValidationErrorCodes.ExactLength => "'{0}' deve essere lungo {1} caratteri.",
        ValidationErrorCodes.LengthBetween => "'{0}' deve essere lungo tra i {1} e {2} caratteri.",
        ValidationErrorCodes.MinLength => "'{0}' deve essere maggiore o uguale a {1} caratteri.",
        ValidationErrorCodes.MaxLength => "'{0}' deve essere minore o uguale a {1} caratteri.",
        _ => null,
    };
}
