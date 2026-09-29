#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class LithuanianLanguage
{
    public const string Culture = "lt";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nėra galiojantis el. pašto adresas.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' turi būti didesnis arba lygus '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' turi būti didesnis už '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' turi būti mažesnis arba lygus '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' turi būti mažesnis už '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' negali būti tuščias.",
        ValidationErrorCodes.NotNull => "'{0}' negali būti tuščias.",
        ValidationErrorCodes.InvalidFormat => "'{0}' nėra tinkamo formato.",
        ValidationErrorCodes.Equal => "'{0}' turi būti lygus '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' turi būti tarp {1} ir {2}. Jūs įvedėte {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' turi būti tarp {1} ir {2} (išskirtinai). Jūs įvedėte {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nėra galiojantis kredito kortelės numeris.",
        ValidationErrorCodes.ScalePrecision => "'{0}' negali turėti daugiau kaip {1} skaitmenų, iš kurių {2} gali būti po kablelio. Rasta {3} skaitmenys ir {4} dešimtainiai.",
        ValidationErrorCodes.LengthBetween => "'{0}' turi būti nuo {1} iki {2} simbolių.",
        ValidationErrorCodes.MinLength => "'{0}' ilgis turi būti bent {1} simbolių.",
        ValidationErrorCodes.MaxLength => "'{0}' ilgis turi būti ne daugiau kaip {1} simbolių.",
        ValidationErrorCodes.ExactLength => "'{0}' ilgis turi būti {1} simbolių.",
        _ => null,
    };
}
