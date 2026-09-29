#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class BosnianLanguage
{
    public const string Culture = "bs";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nije validna email adresa.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' mora biti veće ili jednako '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' mora biti veće od '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' mora biti manje ili jednako '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' mora biti manje od '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ne smije biti prazan.",
        ValidationErrorCodes.NotNull => "'{0}' ne smije biti prazan.",
        ValidationErrorCodes.InvalidFormat => "'{0}' nije u odgovarajućem formatu.",
        ValidationErrorCodes.Equal => "'{0}' mora biti jednak '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' mora biti između {1} i {2}. Uneseno je {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' mora biti između {1} i {2} (ekskluzivno). Uneseno je {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nije validna kreditna kartica.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ne smije imati više od {1} cifara, sa dozvoljenih {2} decimalnih mjesta. Uneseno je {3} cifara i {4} decimalnih mjesta.",
        ValidationErrorCodes.LengthBetween => "'{0}' mora imati između {1} i {2} karaktera.",
        ValidationErrorCodes.MinLength => "'{0}' mora imati najmanje {1} karaktera.",
        ValidationErrorCodes.MaxLength => "'{0}' ne smije imati više od {1} karaktera.",
        ValidationErrorCodes.ExactLength => "'{0}' mora imati tačno {1} karaktera.",
        _ => null,
    };
}
