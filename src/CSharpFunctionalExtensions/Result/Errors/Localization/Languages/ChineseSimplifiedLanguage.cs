#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class ChineseSimplifiedLanguage
{
    public const string Culture = "zh-Hans";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' 不是有效的电子邮件地址。",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' 必须大于或等于 '{1}'。",
        ValidationErrorCodes.GreaterThan => "'{0}' 必须大于 '{1}'。",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' 必须小于或等于 '{1}'。",
        ValidationErrorCodes.LessThan => "'{0}' 必须小于 '{1}'。",
        ValidationErrorCodes.NotEmpty => "'{0}' 不能为空。",
        ValidationErrorCodes.NotNull => "'{0}' 不能为Null。",
        ValidationErrorCodes.InvalidFormat => "'{0}' 的格式不正确。",
        ValidationErrorCodes.Equal => "'{0}' 应该和 '{1}' 相等。",
        ValidationErrorCodes.InclusiveBetween => "'{0}' 必须在 {1} (包含)和 {2} (包含)之间， 您输入了 {3}。",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' 必须在 {1} (不包含)和 {2} (不包含)之间， 您输入了 {3}。",
        ValidationErrorCodes.CreditCard => "'{0}' 不是有效的信用卡号。",
        ValidationErrorCodes.ScalePrecision => "'{0}' 总位数不能超过 {1} 位，其中小数部分 {2} 位。您共计输入了 {3} 位数字，其中小数部分{4} 位。",
        ValidationErrorCodes.LengthBetween => "'{0}' 的长度必须在 {1} 到 {2} 字符。",
        ValidationErrorCodes.MinLength => "'{0}' 必须大于或等于{1}个字符。",
        ValidationErrorCodes.MaxLength => "'{0}' 必须小于或等于{1}个字符。",
        ValidationErrorCodes.ExactLength => "'{0}' 必须是 {1} 个字符。",
        _ => null,
    };
}
