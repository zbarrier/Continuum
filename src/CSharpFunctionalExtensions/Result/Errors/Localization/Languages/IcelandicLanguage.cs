#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class IcelandicLanguage
{
    public const string Culture = "is";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' er ekki gilt netfang.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' verður að vera meiri en eða jöfn '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' verður að vera meiri en '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' verður að vera minna en eða jafnt og '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' verður að vera minna en '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0} má ekki vera tómt.",
        ValidationErrorCodes.NotNull => "'{0} má ekki vera tómt.",
        ValidationErrorCodes.InvalidFormat => "'{0}' er ekki með réttu sniði.",
        ValidationErrorCodes.Equal => "'{0}' verður að vera jafnt og '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' verður að frá {1} til {2}. Þú slóst inn {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' verður að vera á milli {1} og {2}. Þú slóst inn {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' er ekki gilt kreditkortanúmer.",
        ValidationErrorCodes.ScalePrecision => "'{0}' má ekki vera meira en {1} tölustafir samtals, með heimild fyrir {2} aukastöfum. {3} tölustafir og {4} aukastafir fundust.",
        ValidationErrorCodes.LengthBetween => "'{0}' verður að vera á milli {1} og {2} stafir.",
        ValidationErrorCodes.MinLength => "Lengdin '{0}' verður að vera að minnsta kosti {1} stafir.",
        ValidationErrorCodes.MaxLength => "Lengd '{0}' verður að vera {1} stafir eða færri.",
        ValidationErrorCodes.ExactLength => "'{0}' verður að vera {1} stafir að lengd.",
        _ => null,
    };
}
