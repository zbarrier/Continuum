#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class ChineseTraditionalLanguage
{
    public const string Culture = "zh-Hant";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' 不是有效的電子郵件地址。",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' 必須大於或等於 '{1}'。",
        ValidationErrorCodes.GreaterThan => "'{0}' 必須大於 '{1}'。",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' 必須小於或等於 '{1}'。",
        ValidationErrorCodes.LessThan => "'{0}' 必須小於 '{1}'。",
        ValidationErrorCodes.NotEmpty => "'{0}' 不能為空。",
        ValidationErrorCodes.NotNull => "'{0}' 不能為Null。",
        ValidationErrorCodes.InvalidFormat => "'{0}' 的格式不正確。",
        ValidationErrorCodes.Equal => "'{0}' 應該和 '{1}' 相等。",
        ValidationErrorCodes.InclusiveBetween => "'{0}' 必須在 {1} (包含)和 {2} (包含)之間， 您輸入了 {3}。",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' 必須在 {1} (不包含)和 {2} (不包含)之間， 您輸入了 {3}。",
        ValidationErrorCodes.CreditCard => "'{0}' 不是有效的信用卡號碼。",
        ValidationErrorCodes.ScalePrecision => "'{0}' 總位數不能超過 {1} 位，其中小數部份 {2} 位。您共計輸入了 {3} 位數字，其中小數部份{4} 位。",
        ValidationErrorCodes.LengthBetween => "'{0}' 的長度必須在 {1} 到 {2} 字符。",
        ValidationErrorCodes.MinLength => "'{0}' 必須大於或等於{1}個字符。",
        ValidationErrorCodes.MaxLength => "'{0}' 必須小於或等於{1}個字符。",
        ValidationErrorCodes.ExactLength => "'{0}' 必須是 {1} 個字符。",
        _ => null,
    };
}
