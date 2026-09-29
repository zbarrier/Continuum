#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class KoreanLanguage
{
    public const string Culture = "ko";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.CreditCard => "'{0}'이(가) 올바른 신용카드 번호가 아닙니다.",
        ValidationErrorCodes.Email => "'{0}'이(가) 올바른 이메일 주소가 아닙니다.",
        ValidationErrorCodes.Equal => "'{0}'은(는) '{1}'이어야 합니다.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}'은(는) {1} 이상 {2} 미만이어야 합니다. 입력한 값은 {3}입니다.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}'은(는) '{1}'이상이어야 합니다.",
        ValidationErrorCodes.GreaterThan => "'{0}'은(는) '{1}'보다 커야 합니다.",
        ValidationErrorCodes.InclusiveBetween => "'{0}'은(는) {1} 이상 {2} 이하여야 합니다. 입력한 값은 {3}입니다.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}'은(는) '{1}' 이하여야 합니다.",
        ValidationErrorCodes.LessThan => "'{0}'은(는) '{1}' 보다 작아야 합니다.",
        ValidationErrorCodes.NotEmpty => "'{0}'은(는) 최소한 한 글자 이상이어야 합니다.",
        ValidationErrorCodes.NotNull => "'{0}'은(는) 반드시 입력해야 합니다.",
        ValidationErrorCodes.InvalidFormat => "'{0}'이(가) 잘못된 형식입니다.",
        ValidationErrorCodes.ScalePrecision => "'{0}'은(는) 소수점 이하 {2}자리 이하, 총 {1}자리 이하의 숫자여야 합니다. 입력한 값은 소수점 이하 {4}자리이고 총 {3}자리입니다.",
        ValidationErrorCodes.ExactLength => "'{0}'은(는) {1} 글자이하의 문자열이어야 합니다.",
        ValidationErrorCodes.LengthBetween => "'{0}'은(는) {1} 글자 이상 {2} 글자 이하여야 합니다.",
        ValidationErrorCodes.MinLength => "'{0}'은 {1} 자 이상의 값이어야합니다.",
        ValidationErrorCodes.MaxLength => "'{0}'은 (는) {1} 자 이하 여야합니다.",
        _ => null,
    };
}
