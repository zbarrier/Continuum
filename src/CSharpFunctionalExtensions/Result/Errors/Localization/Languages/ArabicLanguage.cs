#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class ArabicLanguage
{
    public const string Culture = "ar";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ليس بريد الكتروني صحيح.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' يجب أن يكون أكبر من أو يساوي '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' يجب أن يكون أكبر من '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' يجب أن يكون أقل من أو يساوي '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' يجب أن يكون أقل من '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' لا يجب أن يكون فارغاً.",
        ValidationErrorCodes.NotNull => "'{0}' لا يجب أن يكون فارغاً.",
        ValidationErrorCodes.InvalidFormat => "'{0}' ليس بالتنسيق الصحيح.",
        ValidationErrorCodes.Equal => "'{0}' يجب أن يساوي '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' يجب أن يكون بين {1} و {2}. ما تم ادخاله {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' يجب أن يكون بين {1} و {2} (حصرياً). ما تم ادخاله {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' ليس رقم بطاقة ائتمان صحيح.",
        ValidationErrorCodes.ScalePrecision => "'{0}' لا يجب أن يكون أكبر من {1} رقما صحيحاً في المجمل, ومسموح بـ {2} أرقام عشرية. ما تم ادخاله {3} أرقام صحيحة و {4} أرقام عشرية.",
        ValidationErrorCodes.LengthBetween => "'{0}' عدد الحروف يجب أن يكون بين {1} و {2}.",
        ValidationErrorCodes.MinLength => "الحد الأدنى لعدد الحروف في '{0}' هو {1}.",
        ValidationErrorCodes.MaxLength => "الحد الأقصى لعدد الحروف في '{0}' هو {1}.",
        ValidationErrorCodes.ExactLength => "الحد الأقصى لعدد الحروف في '{0}' هو {1}.",
        _ => null,
    };
}
