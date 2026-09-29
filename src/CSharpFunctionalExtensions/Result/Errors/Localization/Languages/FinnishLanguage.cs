#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class FinnishLanguage
{
    public const string Culture = "fi";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ei ole kelvollinen sähköpostiosoite.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' pitää olla suurempi tai yhtä suuri kuin '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' pitää olla suurempi kuin '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' pitää olla pienempi tai yhtä suuri kuin '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' pitää olla pienempi kuin '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ei voi olla tyhjä.",
        ValidationErrorCodes.NotNull => "'{0}' ei voi olla tyhjä.",
        ValidationErrorCodes.InvalidFormat => "'{0}' ei ole oikeassa muodossa.",
        ValidationErrorCodes.Equal => "'{0}' pitäisi olla yhtä suuri kuin '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' pitää olla suljetulla välillä {1}-{2}. Syötit {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' pitää olla välillä {1}-{2}. Syötit {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' ei ole kelvollinen luottokortin numero.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ei saa sisältää enempää kuin {1} numeroa, sallien {2} desimaalia. {3} numeroa ja {4} desimaalia löytyi.",
        ValidationErrorCodes.LengthBetween => "'{0}' pitää olla {1}-{2} merkkiä.",
        ValidationErrorCodes.MinLength => "'{0}' saa olla vähintään {1} merkkiä.",
        ValidationErrorCodes.MaxLength => "'{0}' pitää olla enintään {1} merkkiä.",
        ValidationErrorCodes.ExactLength => "'{0}' pitää olla {1} merkkiä pitkä.",
        _ => null,
    };
}
