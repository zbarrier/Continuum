#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class KhmerLanguage
{
    public const string Culture = "km";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}'មិនមែនជាអ៊ីមែលត្រឹមត្រូវទេ។",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}'ត្រូវតែធំជាង ឬស្មើ'{1}។'",
        ValidationErrorCodes.GreaterThan => "'{0}'ត្រូវតែធំជាង'{1}'។",
        ValidationErrorCodes.LessThanOrEqual => "'{0}'ត្រូវតែតិចជាង ឬស្មើនឹង'{1}'។",
        ValidationErrorCodes.LessThan => "'{0}'ត្រូវតែតិចជាង'{1}'។",
        ValidationErrorCodes.NotEmpty => "'{0}'មិនត្រូវទទេទេ។",
        ValidationErrorCodes.NotNull => "'{0}'មិនត្រូវទទេទេ។",
        ValidationErrorCodes.InvalidFormat => "'{0}'មិនមានទម្រង់ត្រឹមត្រូវទេ។",
        ValidationErrorCodes.Equal => "'{0}'ត្រូវតែស្មើនឹង'{1}'។",
        ValidationErrorCodes.InclusiveBetween => "'{0}'ត្រូវតែស្ថិតនៅចន្លោះ{1}និង{2}។ អ្នកបានបញ្ចូល{3}។",
        ValidationErrorCodes.ExclusiveBetween => "'{0}'ត្រូវតែស្ថិតនៅចន្លោះ{1}និង {2}(ដាច់ខាត)។ អ្នកបានបញ្ចូល{3}។",
        ValidationErrorCodes.CreditCard => "'{0}'មិនមែនជាលេខកាតឥណទានត្រឹមត្រូវទេ។",
        ValidationErrorCodes.ScalePrecision => "'{0}'មិនត្រូវលើសពី{1}ខ្ទង់ជាសរុប ដោយមានការអនុញ្ញាតសម្រាប់ទសភាគ{2}។ ទសភាគ{3}ខ្ទង់និង {4}ត្រូវបានរកឃើញ។",
        ValidationErrorCodes.LengthBetween => "'{0}'ត្រូវតែស្ថិតនៅចន្លោះតួអក្សរ{1}ទៅ {2}តួអក្សរ។",
        ValidationErrorCodes.MinLength => "ប្រវែង'{0}'ត្រូវតែមានយ៉ាងហោចណាស់{1}តួអក្សរ។",
        ValidationErrorCodes.MaxLength => "ប្រវែង'{0}'ត្រូវតែមាន{1}តួអក្សរ ឬតិចជាងនេះ។",
        ValidationErrorCodes.ExactLength => "'{0}'ត្រូវតែមាន{1}តួអក្សរ។",
        _ => null,
    };
}
