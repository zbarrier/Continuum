#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class UzbekLatinLanguage
{
    public const string Culture = "uz";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' yaroqli elektron pochta manzili emas.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' kamida '{1}'ga teng bo'lishi shart.",
        ValidationErrorCodes.GreaterThan => "'{0}' '{1}'dan katta bo'lishi shart.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' '{1}'dan kam yoki teng bo'lishi shart.",
        ValidationErrorCodes.LessThan => "'{0}' '{1}'dan kam bo'lishi shart.",
        ValidationErrorCodes.NotEmpty => "'{0}' bo'sh bo'lishi mumkin emas.",
        ValidationErrorCodes.NotNull => "'{0}' bo'sh bo'lishi mumkin emas.",
        ValidationErrorCodes.InvalidFormat => "'{0}' noto'g'ri formatda.",
        ValidationErrorCodes.Equal => "'{0}' '{1}'ga teng bo'lishi shart.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' {1}dan {2}gacha bo'lishi shart. Siz {3} kiritdingiz.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' {1}dan {2}gacha bo'lishi shart (ushbu qiymatlar hisobga olinmaydi). Siz {3} kiritdingiz.",
        ValidationErrorCodes.CreditCard => "'{0}' yaroqli kredit karta raqami emas.",
        ValidationErrorCodes.ScalePrecision => "'{0}' butun qismi jami {1}ta raqamdan oshmasligi shart, jumladan ruxsat etilgan {2} xona kasr(lar) aniqlikda. Butun qismda {3}ta raqam(lar) va {4} xona kasr(lar) topildi.",
        ValidationErrorCodes.LengthBetween => "'{0}' {1}tadan {2}tagacha belgidan iborat bo'lishi shart.",
        ValidationErrorCodes.MinLength => "'{0}' kamida {1}ta belgidan iborat bo'lishi shart.",
        ValidationErrorCodes.MaxLength => "'{0}' ko'pi bilan {1}ta belgidan iborat bo'lishi shart.",
        ValidationErrorCodes.ExactLength => "'{0}' aynan {1}ta belgidan iborat bo'lishi shart.",
        _ => null,
    };
}
