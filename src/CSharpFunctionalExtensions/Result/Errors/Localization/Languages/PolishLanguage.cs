#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class PolishLanguage
{
    public const string Culture = "pl";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "Pole '{0}' nie zawiera poprawnego adresu email.",
        ValidationErrorCodes.GreaterThanOrEqual => "Wartość pola '{0}' musi być równa lub większa niż '{1}'.",
        ValidationErrorCodes.GreaterThan => "Wartość pola '{0}' musi być większa niż '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Wartość pola '{0}' musi być równa lub mniejsza niż '{1}'.",
        ValidationErrorCodes.LessThan => "Wartość pola '{0}' musi być mniejsza niż '{1}'.",
        ValidationErrorCodes.NotEmpty => "Pole '{0}' nie może być puste.",
        ValidationErrorCodes.NotNull => "Pole '{0}' nie może być puste.",
        ValidationErrorCodes.InvalidFormat => "'{0}' wprowadzono w niepoprawnym formacie.",
        ValidationErrorCodes.Equal => "Wartość pola '{0}' musi być równa '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Wartość pola '{0}' musi zawierać się pomiędzy {1} i {2}. Wprowadzono {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Wartość pola '{0}' musi zawierać się pomiędzy {1} i {2} (wyłącznie). Wprowadzono {3}.",
        ValidationErrorCodes.CreditCard => "Pole '{0}' nie zawiera poprawnego numeru karty kredytowej.",
        ValidationErrorCodes.ScalePrecision => "Wartość pola '{0}' nie może mieć więcej niż {1} cyfr z dopuszczalną dokładnością {2} cyfr po przecinku. Znaleziono {3} cyfr i {4} cyfr po przecinku.",
        ValidationErrorCodes.LengthBetween => "Długość pola '{0}' musi zawierać się pomiędzy {1} i {2} znaki(ów).",
        ValidationErrorCodes.MinLength => "Długość pola '{0}' musi być większa lub równa {1} znaki(ów).",
        ValidationErrorCodes.MaxLength => "Długość pola '{0}' musi być mniejsza lub równa {1} znaki(ów).",
        ValidationErrorCodes.ExactLength => "Pole '{0}' musi posiadać długość {1} znaki(ów).",
        _ => null,
    };
}
