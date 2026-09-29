#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class SpanishLanguage
{
    public const string Culture = "es";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' no es una dirección de correo electrónico válida.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' debe ser mayor o igual que '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' debe ser mayor que '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' debe ser menor o igual que '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' debe ser menor que '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' no debería estar vacío.",
        ValidationErrorCodes.NotNull => "'{0}' no debe estar vacío.",
        ValidationErrorCodes.InvalidFormat => "'{0}' no tiene el formato correcto.",
        ValidationErrorCodes.Equal => "'{0}' debería ser igual a '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' debe estar entre {1} y {2} (exclusivo). Actualmente tiene un valor de {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' debe estar entre {1} y {2}. Actualmente tiene un valor de {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' no es un número de tarjeta de crédito válido.",
        ValidationErrorCodes.ScalePrecision => "'{0}' no debe tener más de {1} dígitos en total, con margen para {2} decimales. Se encontraron {3} y {4} decimales.",
        ValidationErrorCodes.LengthBetween => "'{0}' debe tener entre {1} y {2} caracteres.",
        ValidationErrorCodes.MinLength => "'{0}' debe ser mayor o igual que {1} caracteres.",
        ValidationErrorCodes.MaxLength => "'{0}' debe ser menor o igual que {1} caracteres.",
        ValidationErrorCodes.ExactLength => "'{0}' debe tener una longitud de {1} caracteres.",
        _ => null,
    };
}
