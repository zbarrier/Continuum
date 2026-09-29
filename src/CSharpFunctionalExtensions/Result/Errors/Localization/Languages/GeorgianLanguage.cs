#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class GeorgianLanguage
{
    public const string Culture = "ka";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' არ არის ვალიდური ელ.ფოსტის მისამართი.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' უნდა იყოს '{1}'-ზე მეტი ან ტოლი.",
        ValidationErrorCodes.GreaterThan => "'{0}' უნდა იყოს '{1}'-ზე მეტი.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' უნდა იყოს '{1}'-ზე ნაკლები ან ტოლი.",
        ValidationErrorCodes.LessThan => "'{0}' უნდა იყოს '{1}'-ზე ნაკლები.",
        ValidationErrorCodes.NotEmpty => "'{0}' არ უნდა იყოს ცარიელი.",
        ValidationErrorCodes.NotNull => "'{0}' არ უნდა იყოს ცარიელი.",
        ValidationErrorCodes.InvalidFormat => "'{0}'-ის ფორმატი არასწორია.",
        ValidationErrorCodes.Equal => "'{0}' უნდა უდრიდეს '{1}'-ს.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' უნდა იყოს {1}-დან {2}-მდე (ჩათვლით). თქვენ შეიყვანეთ {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' უნდა იყოს {1}-სა და {2}-ს შორის. თქვენ შეიყვანეთ {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' არ არის ვალიდური საკრედიტო ბარათის ნომერი.",
        ValidationErrorCodes.ScalePrecision => "'{0}' არ უნდა იყოს ჯამში {1} ციფრზე მეტი, {2} ათობითი ციფრის ჩათვლით. თქვენ შეიყვანეთ {3} ციფრი და {4} ათობითი სიმბოლო.",
        ValidationErrorCodes.LengthBetween => "'{0}' უნდა იყოს {1}-დან {2} სიმბოლომდე.",
        ValidationErrorCodes.MinLength => "'{0}'-ის სიგრძე უნდა აღემატებოდეს {1} სიმბოლოს.",
        ValidationErrorCodes.MaxLength => "'{0}'-ის სიგრძე არ უნდა აღემატებოდეს {1} სიმბოლოს.",
        ValidationErrorCodes.ExactLength => "'{0}' უნდა უდრიდეს {1} სიმბოლოს.",
        _ => null,
    };
}
