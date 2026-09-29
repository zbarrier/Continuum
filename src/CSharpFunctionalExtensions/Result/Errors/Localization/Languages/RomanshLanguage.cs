#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class RomanshLanguage
{
    public const string Culture = "rm";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' n'è betg ina adressa email valaibla.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' duai esser pli grond u uguagli a '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' duai esser pli grond che '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' duai esser pli pitschen u uguagli a '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' duai esser pli pitschen che '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' n'ha betg da vegnir mussà.",
        ValidationErrorCodes.NotNull => "'{0}' n'ha betg da vegnir mussà.",
        ValidationErrorCodes.InvalidFormat => "'{0}' n'è betg en il format correct.",
        ValidationErrorCodes.Equal => "'{0}' duai esser uguaglià cun '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' duai esser tranter {1} e {2} (exclusiv). Vus avais mess {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' duai esser tranter {1} e {2}. Vus avais mess {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' n'è betg ina cifra da carta da credit valaibla.",
        ValidationErrorCodes.ScalePrecision => "'{0}' n'ha betg pli che {1} cifras en total, cun toleranza per {2} decimalas. Sunt vegnidas chattadas {3} cifras e {4} decimalas.",
        ValidationErrorCodes.LengthBetween => "'{0}' duai esser lunga tranter {1} e {2} caracters.",
        ValidationErrorCodes.MinLength => "La lunghezza da '{0}' duai esser almain {1} caracters.",
        ValidationErrorCodes.MaxLength => "La lunghezza da '{0}' po esser massim {1} caracters.",
        ValidationErrorCodes.ExactLength => "'{0}' duai esser lunga {1} caracters.",
        _ => null,
    };
}
