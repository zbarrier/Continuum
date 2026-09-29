#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class TurkishLanguage
{
    public const string Culture = "tr";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}'  geçerli bir e-posta adresi değil.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' değeri '{1}' değerinden büyük veya eşit olmalı.",
        ValidationErrorCodes.GreaterThan => "'{0}' değeri '{1}' değerinden büyük olmalı.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}', '{1}' değerinden küçük veya eşit olmalı.",
        ValidationErrorCodes.LessThan => "'{0}', '{1}' değerinden küçük olmalı.",
        ValidationErrorCodes.NotEmpty => "'{0}' boş olmamalı.",
        ValidationErrorCodes.NotNull => "'{0}' boş olamaz.",
        ValidationErrorCodes.InvalidFormat => "'{0}' değerinin formatı doğru değil.",
        ValidationErrorCodes.Equal => "'{0}', '{1}' değerine eşit olmalı.",
        ValidationErrorCodes.InclusiveBetween => "'{0}', {1} ve {2} arasında olmalı. {3} değerini girdiniz.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}', {1} ve {2} (dahil değil) arasında olmalı. {3} değerini girdiniz.",
        ValidationErrorCodes.CreditCard => "'{0}' geçerli bir kredi kartı numarası değil.",
        ValidationErrorCodes.ScalePrecision => "'{0}', {2} ondalıkları için toplamda {1} rakamdan fazla olamaz. {3} basamak ve {4} basamak bulundu.",
        ValidationErrorCodes.LengthBetween => "'{0}', {1} ve {2} arasında karakter uzunluğunda olmalı.",
        ValidationErrorCodes.MinLength => "'{0}', {1} karakterden büyük veya eşit olmalıdır.",
        ValidationErrorCodes.MaxLength => "'{0}', {1} karakterden küçük veya eşit olmalıdır.",
        ValidationErrorCodes.ExactLength => "'{0}', {1} karakter uzunluğunda olmalı.",
        _ => null,
    };
}
