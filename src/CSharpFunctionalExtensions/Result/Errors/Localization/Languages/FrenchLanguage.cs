#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class FrenchLanguage
{
    public const string Culture = "fr";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' n'est pas une adresse email valide.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' doit être plus grand ou égal à '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' doit être plus grand que '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' doit être plus petit ou égal à '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' doit être plus petit que '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ne doit pas être vide.",
        ValidationErrorCodes.NotNull => "'{0}' ne doit pas avoir la valeur null.",
        ValidationErrorCodes.InvalidFormat => "'{0}' n'a pas le bon format.",
        ValidationErrorCodes.Equal => "'{0}' doit être égal à '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' doit être entre {1} et {2} (exclusif). Vous avez saisi {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' doit être entre {1} et {2}. Vous avez saisi {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' n'est pas un numéro de carte de crédit valide.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ne doit pas dépasser {1} chiffres au total, avec une tolérance de {2} décimales. {3} nombres entiers et {4} décimales ont été trouvés.",
        ValidationErrorCodes.LengthBetween => "'{0}' doit contenir entre {1} et {2} caractères.",
        ValidationErrorCodes.MinLength => "'{0}' doit être supérieur ou égal à {1} caractères.",
        ValidationErrorCodes.MaxLength => "'{0}' doit être inférieur ou égal à {1} caractères.",
        ValidationErrorCodes.ExactLength => "'{0}' doit être d’une longueur de {1} caractères.",
        _ => null,
    };
}
