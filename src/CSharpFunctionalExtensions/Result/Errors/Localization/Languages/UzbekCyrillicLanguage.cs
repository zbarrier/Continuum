#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class UzbekCyrillicLanguage
{
    public const string Culture = "uz-Cyrl-UZ";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' яроқли электрон почта манзили эмас.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' камида '{1}'га тенг бўлиши шарт.",
        ValidationErrorCodes.GreaterThan => "'{0}' '{1}'дан катта бўлиши шарт.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' '{1}'дан кам ёки тенг бўлиши шарт.",
        ValidationErrorCodes.LessThan => "'{0}' '{1}'дан кам бўлиши шарт.",
        ValidationErrorCodes.NotEmpty => "'{0}' бўш бўлиши мумкин эмас.",
        ValidationErrorCodes.NotNull => "'{0}' бўш бўлиши мумкин эмас.",
        ValidationErrorCodes.InvalidFormat => "'{0}' нотўғри форматда.",
        ValidationErrorCodes.Equal => "'{0}' '{1}'га тенг бўлиши шарт.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' {1}дан {2}гача бўлиши шарт. Сиз {3} киритдингиз.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' {1}дан {2}гача бўлиши шарт (ушбу қийматлар ҳисобга олинмайди). Сиз {3} киритдингиз.",
        ValidationErrorCodes.CreditCard => "'{0}' яроқли кредит карта рақами эмас.",
        ValidationErrorCodes.ScalePrecision => "'{0}' бутун қисми жами {1}та рақамдан ошмаслиги шарт, жумладан рухсат этилган {2} хона каср(лар) аниқликда. Бутун қисмда {3}та рақам(лар) ва {4} хона каср(лар) топилди.",
        ValidationErrorCodes.LengthBetween => "'{0}' {1}тадан {2}тагача белгидан иборат бўлиши шарт.",
        ValidationErrorCodes.MinLength => "'{0}' камида {1}та белгидан иборат бўлиши шарт.",
        ValidationErrorCodes.MaxLength => "'{0}' кўпи билан {1}та белгидан иборат бўлиши шарт.",
        ValidationErrorCodes.ExactLength => "'{0}' айнан {1}та белгидан иборат бўлиши шарт.",
        _ => null,
    };
}
