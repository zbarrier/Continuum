#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class MacedonianLanguage
{
    public const string Culture = "mk";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' не е валидна емаил адреса.",
        ValidationErrorCodes.GreaterThanOrEqual => "Вредноста на '{0}' мора да биде поголема или еднаква на '{1}'.",
        ValidationErrorCodes.GreaterThan => "Вредноста на '{0}' мора да биде поголема од '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Вредноста на '{0}' мора да биде помала или еднаква на '{1}'.",
        ValidationErrorCodes.LessThan => "Вредноста на '{0}' мора да биде помала од '{1}'.",
        ValidationErrorCodes.NotEmpty => "Вредноста на '{0}' не треба да биде празна.",
        ValidationErrorCodes.NotNull => "Вредноста на '{0}' не треба да биде празна.",
        ValidationErrorCodes.InvalidFormat => "'{0}' не е во правилниот формат.",
        ValidationErrorCodes.Equal => "Вредноста на '{0}' би требало да биде еднаква на '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Вредноста на '{0}' мора да биде помеѓу {1} и {2}. Имате внесено {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Вредноста на '{0}' мора да биде од {1} до {2} (исклучително). Имате внесено вредност {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' не е валиден бројот на кредитната картичка.",
        ValidationErrorCodes.ScalePrecision => "'{0}' не би требало да биде повеќе од  {1} цифри вкупно, со дозволени  {2} децимали. {3} цифри и {4} децимали беа најдени.",
        ValidationErrorCodes.LengthBetween => "Должината на '{0}' мора да биде помеѓу {1} и {2} карактери.",
        ValidationErrorCodes.MinLength => "Должината на '{0}' мора да биде поголема или еднаква на {1} знаци.",
        ValidationErrorCodes.MaxLength => "Должината на '{0}' мора да биде помала или еднаква на {1} знаци.",
        ValidationErrorCodes.ExactLength => "Должината на '{0}' мора да биде {1} карактери.",
        _ => null,
    };
}
