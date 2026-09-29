#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class TajikLanguage
{
    public const string Culture = "tg";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' суроғаи почтаи электронии дуруст нест.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' бояд аз '{1}' бузургтар ё баробар бошад.",
        ValidationErrorCodes.GreaterThan => "'{0}' бояд аз '{1}' бузургтар бошад.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' бояд аз '{1}' камтар ё баробар бошад.",
        ValidationErrorCodes.LessThan => "'{0}' бояд камтар аз '{1}' бошад.",
        ValidationErrorCodes.NotEmpty => "'{0}' набояд холӣ бошад.",
        ValidationErrorCodes.NotNull => "'{0}' набояд холӣ бошад.",
        ValidationErrorCodes.InvalidFormat => "'{0}' дар формати дуруст нест.",
        ValidationErrorCodes.Equal => "'{0}' бояд ба '{1}' баробар бошад.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' бояд байни {1} ва {2} бошад. Шумо {3}-ро ворид кардаед.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' бояд байни {1} ва {2} (ба истиснои ҳаминҳо) бошад. Шумо {3}-ро ворид кардаед.",
        ValidationErrorCodes.CreditCard => "'{0}' рақами дурусти корти кредитӣ нест.",
        ValidationErrorCodes.ScalePrecision => "'{0}' набояд дар маҷмӯъ аз рақамҳои {1} зиёд бошад, бо назардошти он барои даҳҳо {2}. {3} рақам ва {4} даҳӣ ёфт шуданд.",
        ValidationErrorCodes.LengthBetween => "'{0}' бояд дар байни {1} ва {2} аломат бошад.",
        ValidationErrorCodes.MinLength => "Дарозии '{0}' бояд ҳадди аққал {1} аломат бошад.",
        ValidationErrorCodes.MaxLength => "Дарозии '{0}' бояд {1} аломат ё камтар бошад.",
        ValidationErrorCodes.ExactLength => "'{0}' бояд дарозии {1} аломат бошад.",
        _ => null,
    };
}
