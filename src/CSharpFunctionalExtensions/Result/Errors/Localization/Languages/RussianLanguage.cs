#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class RussianLanguage
{
    public const string Culture = "ru";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' неверный email адрес.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' должно быть больше или равно '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' должно быть больше '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' должно быть меньше или равно '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' должно быть меньше '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' должно быть заполнено.",
        ValidationErrorCodes.NotNull => "'{0}' должно быть заполнено.",
        ValidationErrorCodes.InvalidFormat => "'{0}' имеет неверный формат.",
        ValidationErrorCodes.Equal => "'{0}' должно быть равно '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' должно быть в диапазоне от {1} до {2}. Введенное значение: {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' должно быть в диапазоне от {1} до {2} (не включая эти значения). Введенное значение: {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' неверный номер карты.",
        ValidationErrorCodes.ScalePrecision => "'{0}' должно содержать не более {1} цифр всего, в том числе {2} десятичных знака(ов). Введенное значение содержит {3} цифр(ы) и {4} десятичных знака(ов).",
        ValidationErrorCodes.LengthBetween => "'{0}' должно быть длиной от {1} до {2} символов.",
        ValidationErrorCodes.MinLength => "'{0}' должно быть длиной не менее {1} символов.",
        ValidationErrorCodes.MaxLength => "'{0}' должно быть длиной не более {1} символов.",
        ValidationErrorCodes.ExactLength => "'{0}' должно быть длиной {1} символа(ов).",
        _ => null,
    };
}
