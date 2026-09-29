#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class SerbianCyrillicLanguage
{
    public const string Culture = "sr";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' није валидна email адреса.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' мора бити веће или једнако од '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' мора бити веће од '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' мора бити мање или једнако од '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' мора бити мање од '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' не сме бити празно.",
        ValidationErrorCodes.NotNull => "'{0}' не сме бити празно.",
        ValidationErrorCodes.InvalidFormat => "'{0}' није у одговарајућем формату.",
        ValidationErrorCodes.Equal => "'{0}' мора бити једнако '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' мора бити између {1} и {2}. Унесено је {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' мора бити између {1} и {2} (ексклузивно). Унесено је {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' није валидан број кредитне картице.",
        ValidationErrorCodes.ScalePrecision => "'{0}' не сме имати више од {1} цифара, са дозвољених {2} децималних места. Унесено је {3} цифара и {4} децималних места.",
        ValidationErrorCodes.LengthBetween => "'{0}' мора имати између {1} и {2} карактера.",
        ValidationErrorCodes.MinLength => "'{0}' мора имати најмање {1} карактера.",
        ValidationErrorCodes.MaxLength => "'{0}' не сме имати више од {1} карактера.",
        ValidationErrorCodes.ExactLength => "'{0}' мора имати тачно {1} карактера.",
        _ => null,
    };
}
