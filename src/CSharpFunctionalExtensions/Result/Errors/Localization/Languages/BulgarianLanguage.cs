#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class BulgarianLanguage
{
    public const string Culture = "bg";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' не е валиден е-мейл адрес.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' трябва да бъде по-голямо или равно на  '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' трябва да бъде по-голямо от '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' трябва да бъде по-малко или равно на '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' трябва да бъде по-малко от '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' не трябва да бъде празно.",
        ValidationErrorCodes.NotNull => "'{0}' не трябва да бъде празно.",
        ValidationErrorCodes.InvalidFormat => "'{0}' не е в правилния формат.",
        ValidationErrorCodes.Equal => "'{0}' трябва да бъде равно на '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' трябва да бъде между {1} и {2}. Вие въведохте {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' трябва да бъде между {1} и {2} (изключително). Вие въведохте {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' не е валиден номер на кредитна карта.",
        ValidationErrorCodes.ScalePrecision => "'{0}' не трябва да е повече от {1} цифри и трябва да бъде до {2} знака след запетаята. В момента има {3} цифри и {4} знака след запетаята.",
        ValidationErrorCodes.LengthBetween => "'{0}' трябва да бъде межди {1} и {2} брой символи.",
        ValidationErrorCodes.MinLength => "Дължината на '{0}' трябва да бъде поне {1} символи.",
        ValidationErrorCodes.MaxLength => "Дължината на '{0}' трябва да бъде {1} или по-малко брой символи.",
        ValidationErrorCodes.ExactLength => "'{0}' трябва да бъде {1} дължина на символите.",
        _ => null,
    };
}
