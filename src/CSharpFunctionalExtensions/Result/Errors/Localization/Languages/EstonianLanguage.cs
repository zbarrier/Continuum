#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class EstonianLanguage
{
    public const string Culture = "et";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ei ole sobiv e-posti aadress.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' peab olema suurem või sama suur kui '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' peab olema suurem kui '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' peab olema väiksem või sama suur kui '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' peab olema väiksem kui '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ei või olla tühi.",
        ValidationErrorCodes.NotNull => "'{0}' ei või olla tühi.",
        ValidationErrorCodes.InvalidFormat => "'{0}' ei ole õige kujuga.",
        ValidationErrorCodes.Equal => "'{0}' peab olema sama väärtusega nagu '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' peab olema vahemikus {1}-{2}. Sisestasid {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' peab olema vahemikus {1}-{2}. Sisestasid {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' ei ole sobiv krediitkaardi number.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ei tohi olla pikem kui {1} numbrit, {2} kümndendikku. Sisestatud {3} numbrit ja {4} kümnendikku.",
        ValidationErrorCodes.LengthBetween => "'{0}' peab olema {1}-{2} märki.",
        ValidationErrorCodes.MinLength => "'{0}' pikkus peab olema vähemalt {1} märki.",
        ValidationErrorCodes.MaxLength => "'{0}' võib olla kõige rohkem {1} märki.",
        ValidationErrorCodes.ExactLength => "'{0}' peab olema {1} märgi pikkune.",
        _ => null,
    };
}
