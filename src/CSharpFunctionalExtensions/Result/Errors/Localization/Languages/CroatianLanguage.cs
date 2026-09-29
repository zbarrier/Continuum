#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class CroatianLanguage
{
    public const string Culture = "hr";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nije ispravna e-mail adresa.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' mora biti veći ili jednak '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' mora biti veći od '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' mora biti manji ili jednak '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' mora biti manji od '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ne smije biti prazan.",
        ValidationErrorCodes.NotNull => "Niste upisali '{0}'",
        ValidationErrorCodes.InvalidFormat => "'{0}' nije u odgovarajućem formatu.",
        ValidationErrorCodes.Equal => "'{0}' mora biti jednak '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' mora biti između {1} i {2}. Upisali ste {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' mora biti između {1} i {2} (ne uključujući granice). Upisali ste {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nije odgovarajuća kreditna kartica.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ne smije imati više od {1} znamenki, sa {2} decimalna mjesta. Upisali ste {3} znamenki i {4} decimalna mjesta.",
        ValidationErrorCodes.LengthBetween => "'{0}' mora biti između {1} i {2} znakova.",
        ValidationErrorCodes.MinLength => "'{0}' mora imati duljinu veću ili jednaku {1}.",
        ValidationErrorCodes.MaxLength => "'{0}' mora imati duljinu manju ili jednaku {1}.",
        ValidationErrorCodes.ExactLength => "'{0}' mora sadržavati {1} znakova.",
        _ => null,
    };
}
