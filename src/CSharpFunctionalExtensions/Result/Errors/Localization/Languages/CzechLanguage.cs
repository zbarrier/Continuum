#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class CzechLanguage
{
    public const string Culture = "cs";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "Pole '{0}' musí obsahovat platnou emailovou adresu.",
        ValidationErrorCodes.GreaterThanOrEqual => "Hodnota pole '{0}' musí být větší nebo rovna '{1}'.",
        ValidationErrorCodes.GreaterThan => "Hodnota pole '{0}' musí být větší než '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Hodnota pole '{0}' musí být menší nebo rovna '{1}'.",
        ValidationErrorCodes.LessThan => "Hodnota pole '{0}' musí být menší než '{1}'.",
        ValidationErrorCodes.NotEmpty => "Pole '{0}' nesmí být prázdné.",
        ValidationErrorCodes.NotNull => "Pole '{0}' nesmí být prázdné.",
        ValidationErrorCodes.InvalidFormat => "Pole '{0}' nemá správný formát.",
        ValidationErrorCodes.Equal => "Hodnota pole '{0}' musí být rovna '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Hodnota pole '{0}' musí být mezi {1} a {2} (včetně). Vámi zadaná hodnota je {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Hodnota pole '{0}' musí být větší než {1} a menší než {2}. Vámi zadaná hodnota je {3}.",
        ValidationErrorCodes.CreditCard => "Pole '{0}' musí obsahovat platné číslo platební karty.",
        ValidationErrorCodes.ScalePrecision => "Pole '{0}' nesmí mít víc než {1} číslic a {2} desetinných míst. Vámi bylo zadáno {3} číslic a {4} desetinných míst.",
        ValidationErrorCodes.LengthBetween => "Délka pole '{0}' musí být v rozsahu {1} až {2} znaků.",
        ValidationErrorCodes.MinLength => "Délka pole '{0}' musí být větší nebo rovna {1} znakům.",
        ValidationErrorCodes.MaxLength => "Délka pole '{0}' musí být menší nebo rovna {1} znakům.",
        ValidationErrorCodes.ExactLength => "Délka pole '{0}' musí být {1} znaků.",
        _ => null,
    };
}
