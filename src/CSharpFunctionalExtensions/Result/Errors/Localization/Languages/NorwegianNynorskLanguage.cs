#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class NorwegianNynorskLanguage
{
    public const string Culture = "nn";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' er ikkje ei gyldig e-postadresse.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' skal vera større enn eller lik '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' skal vera større enn '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' skal vera mindre enn eller lik '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' skal vera mindre enn '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' kan ikkje vera tom.",
        ValidationErrorCodes.NotNull => "'{0}' kan ikkje vera tom.",
        ValidationErrorCodes.InvalidFormat => "'{0}' har ikkje rett format.",
        ValidationErrorCodes.Equal => "'{0}' skal vera lik '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' skal vera mellom {1} og {2}. Du har tasta inn {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' skal vera mellom {1} og {2} (unntatt). Du har tasta inn {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' er ikkje eit gyldig kredittkortnummer.",
        ValidationErrorCodes.ScalePrecision => "'{0}' kan ikkje vera meir enn {1} siffer totalt, inkludert {2} desimalar. {3} siffer og {4} desimalar vart funnen.",
        ValidationErrorCodes.LengthBetween => "'{0}' skal vera mellom {1} og {2} teikn.",
        ValidationErrorCodes.MinLength => "'{0}' skal vera større enn eller lik {1} teikn.",
        ValidationErrorCodes.MaxLength => "'{0}' skal vera mindre enn eller lik {1} teikn.",
        ValidationErrorCodes.ExactLength => "'{0}' skal vera {1} teikn langt.",
        _ => null,
    };
}
