#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class VietnameseLanguage
{
    public const string Culture = "vi";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' không phải là một email hợp lệ.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' phải lớn hơn hoặc bằng với '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' phải lớn hơn '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' phải nhỏ hơn hoặc bằng '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' phải nhỏ hơn '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' không được rỗng.",
        ValidationErrorCodes.NotNull => "'{0}' phải có giá trị.",
        ValidationErrorCodes.InvalidFormat => "'{0}' không đúng định dạng.",
        ValidationErrorCodes.Equal => "'{0}' phải bằng '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' phải có giá trị trong khoảng từ {1} đến {2}. Bạn đã nhập {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' phải có giá trị trong khoảng giữa {1} và {2} (không bao gồm hai giới hạn). Bạn đã nhập {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' không đúng định dạng thẻ tín dụng.",
        ValidationErrorCodes.ScalePrecision => "'{0}' không được vượt quá {1} chữ số tổng cộng và {2} chữ số phần thập phân. Phát hiện {3} chữ số và {4} chữ số phần thập phân.",
        ValidationErrorCodes.LengthBetween => "'{0}' phải nằm trong khoảng từ {1} đến {2} kí tự.",
        ValidationErrorCodes.MinLength => "Độ dài tối thiểu của '{0}' phải là {1} kí tự.",
        ValidationErrorCodes.MaxLength => "Độ dài tối đa của '{0}' phải là {1} kí tự hoặc ít hơn.",
        ValidationErrorCodes.ExactLength => "'{0}' phải có độ dài chính xác {1} kí tự.",
        _ => null,
    };
}
