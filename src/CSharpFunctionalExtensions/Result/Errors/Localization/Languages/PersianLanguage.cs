#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class PersianLanguage
{
    public const string Culture = "fa";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' وارد شده قالب صحیح یک ایمیل را ندارد.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' باید بیشتر یا مساوی '{1}' باشد.",
        ValidationErrorCodes.GreaterThan => "'{0}' باید بیشتر از '{1}' باشد.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' باید کمتر یا مساوی '{1}' باشد.",
        ValidationErrorCodes.LessThan => "'{0}' باید کمتر از '{1}' باشد.",
        ValidationErrorCodes.NotEmpty => "وارد کردن '{0}' ضروری است.",
        ValidationErrorCodes.NotNull => "وارد کردن '{0}' ضروری است.",
        ValidationErrorCodes.InvalidFormat => "'{0}' دارای قالب صحیح نیست.",
        ValidationErrorCodes.Equal => "مقادیر وارد شده برای '{0}' و '{1}' یکسان نیستند.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' باید بین {1} و {2} باشد. اما مقدار وارد شده ({3}) در این محدوده نیست.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' باید بیشتر از {1} و کمتر از {2} باشد. اما مقدار وارد شده ({3}) در این محدوده نیست.",
        ValidationErrorCodes.CreditCard => "'{0}' وارد شده معتبر نیست.",
        ValidationErrorCodes.ScalePrecision => "'{0}' نباید بیش از {1} رقم، شامل {2} رقم اعشار داشته باشد. مقدار وارد شده {3} رقم و {4} رقم اعشار دارد.",
        ValidationErrorCodes.LengthBetween => "'{0}' باید حداقل {1} و حداکثر {2} کاراکتر داشته باشد.",
        ValidationErrorCodes.MinLength => "'{0}' باید بزرگتر یا برابر با {1} کاراکتر باشد.",
        ValidationErrorCodes.MaxLength => "'{0}' باید کمتر یا مساوی {1} باشد.",
        ValidationErrorCodes.ExactLength => "'{0}' باید دقیقا {1} کاراکتر.",
        _ => null,
    };
}
