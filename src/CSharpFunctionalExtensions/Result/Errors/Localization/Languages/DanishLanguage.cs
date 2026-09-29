#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class DanishLanguage
{
    public const string Culture = "da";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' er ikke en gyldig e-mail-adresse.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' skal være større end eller lig med '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' skal være større end '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' skal være mindre end eller lig med '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' skal være mindre end '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' må ikke være tom.",
        ValidationErrorCodes.NotNull => "'{0}' må ikke være tom.",
        ValidationErrorCodes.InvalidFormat => "'{0}' er ikke i det rigtige format.",
        ValidationErrorCodes.Equal => "'{0}' skal være lig med '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' skal være mellem {1} og {2}. Du har indtastet {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' skal være mellem {1} og {2} (eksklusiv). Du har indtastet {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' er ikke et gyldigt kreditkortnummer.",
        ValidationErrorCodes.ScalePrecision => "'{0}' må ikke være mere end {1} cifre i alt, med hensyn til {2} decimaler. {3} cifre og {4} decimaler blev fundet.",
        ValidationErrorCodes.LengthBetween => "'{0}' skal være mellem {1} og {2} tegn.",
        ValidationErrorCodes.MinLength => "'{0}' skal være større end eller lig med {1} tegn.",
        ValidationErrorCodes.MaxLength => "'{0}' skal være mindre end eller lig med {1} tegn.",
        ValidationErrorCodes.ExactLength => "'{0}' skal være {1} tegn langt.",
        _ => null,
    };
}
