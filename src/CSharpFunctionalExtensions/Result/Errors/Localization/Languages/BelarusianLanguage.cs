#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class BelarusianLanguage
{
    public const string Culture = "be";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' некарэктны email адрас.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' павінна быць больш або роўна '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' павінна быць больш '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' павінна быць менш або роўна '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' павінна быць менш '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' павінна быць запоўнена.",
        ValidationErrorCodes.NotNull => "'{0}' павінна быць вызначана.",
        ValidationErrorCodes.InvalidFormat => "'{0}' не адпавядае вызначанаму фармату.",
        ValidationErrorCodes.Equal => "'{0}' павінна быць аднолькава з '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' павінна быць у дыяпазоне ад {1} да {2}. Уведзенае значэнне: {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' павінна быць у дыяпазоне ад {1} да {2} (не ўключаючы гэтыя значэнні). Уведзенае значэнне: {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' некарэктны нумар карты.",
        ValidationErrorCodes.ScalePrecision => "'{0}' павінна ўтрымліваць не больш за {1} лічбаў усяго, у тым ліку {2} дзесятковых знакаў. Уведзенае значэнне ўтрымлівае {3} лічбаў і {4} дзесятковых знакаў.",
        ValidationErrorCodes.LengthBetween => "'{0}' павінна мець даўжыню ад {1} да {2} сімвалаў.",
        ValidationErrorCodes.MinLength => "'{0}' павінна мець даўжыню не менш за {1} сімвалаў.",
        ValidationErrorCodes.MaxLength => "'{0}' павінна мець даўжыню не больш за {1} сімвалаў.",
        ValidationErrorCodes.ExactLength => "'{0}' павінна мець даўжыню {1} сімвала(ў).",
        _ => null,
    };
}
