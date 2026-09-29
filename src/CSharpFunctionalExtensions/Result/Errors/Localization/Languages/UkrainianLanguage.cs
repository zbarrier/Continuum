#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class UkrainianLanguage
{
    public const string Culture = "uk";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' не є email-адресою.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' має бути більшим, або дорівнювати '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' має бути більшим за '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' має бути меншим, або дорівнювати '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' має бути меншим за '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' не може бути порожнім.",
        ValidationErrorCodes.NotNull => "'{0}' не може бути порожнім.",
        ValidationErrorCodes.InvalidFormat => "'{0}' має неправильний формат.",
        ValidationErrorCodes.Equal => "'{0}' має дорівнювати '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' має бути між {1} та {2} (включно). Ви ввели {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' має бути між {1} та {2}. Ви ввели {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' не є номером кредитної картки.",
        ValidationErrorCodes.ScalePrecision => "'{0}' не може мати більше за {1} цифр всього, з {2} десятковими знаками. {3} цифр та {4} десяткових знаків знайдено.",
        ValidationErrorCodes.LengthBetween => "'{0}' має бути довжиною від {1} до {2} символів.",
        ValidationErrorCodes.MinLength => "Довжина '{0}' має бути не меншою ніж {1} символів.",
        ValidationErrorCodes.MaxLength => "Довжина '{0}' має бути {1} символів, або менше.",
        ValidationErrorCodes.ExactLength => "'{0}' має бути довжиною {1} символів.",
        _ => null,
    };
}
