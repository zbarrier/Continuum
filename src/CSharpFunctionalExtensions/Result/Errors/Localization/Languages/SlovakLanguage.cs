#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class SlovakLanguage
{
    public const string Culture = "sk";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "Pole '{0}' musí obsahovať platnú emailovú adresu.",
        ValidationErrorCodes.GreaterThanOrEqual => "Hodnota poľa '{0}' musí byť väčšia alebo sa rovnať '{1}'.",
        ValidationErrorCodes.GreaterThan => "Hodnota poľa '{0}' musí byť väčšia ako '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Hodnota poľa '{0}' musí byť menšia alebo sa rovnať '{1}'.",
        ValidationErrorCodes.LessThan => "Hodnota poľa '{0}' musí byť menšia ako '{1}'.",
        ValidationErrorCodes.NotEmpty => "Pole '{0}' nesmie byť prázdne.",
        ValidationErrorCodes.NotNull => "Pole '{0}' nesmie byť prázdne.",
        ValidationErrorCodes.InvalidFormat => "Pole '{0}' nemá správný formát.",
        ValidationErrorCodes.Equal => "Hodnota poľa '{0}' musí byť rovná '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Hodnota poľa '{0}' musí byť medzi {1} a {2} (vrátane). Vami zadaná hodnota je {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Hodnota poľa '{0}' musí byť väčšia ako {1} a menšia ako {2}. Vami zadaná hodnota {3}.",
        ValidationErrorCodes.CreditCard => "Pole '{0}' nie je správné číslo kreditnej karty.",
        ValidationErrorCodes.ScalePrecision => "Pole '{0}' nemôže mať viac  ako {1} čísiel a {2} desatinných miest. Vami bolo zadané {3} číslic a {4} desatinných miest.",
        ValidationErrorCodes.LengthBetween => "Dĺžka poľa '{0}' musí byť medzi {1} a {2} znakmi.",
        ValidationErrorCodes.MinLength => "Dĺžka poľa '{0}' musí byť väčšia alebo rovná {1} znakom.",
        ValidationErrorCodes.MaxLength => "Dĺžka poľa '{0}' musí byť menšia alebo rovná {1} znakom.",
        ValidationErrorCodes.ExactLength => "Dĺžka poľa '{0}' musí byť {1} znakov. ",
        _ => null,
    };
}
