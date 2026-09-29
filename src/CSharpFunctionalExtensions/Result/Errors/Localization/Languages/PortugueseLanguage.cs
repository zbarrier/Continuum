#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class PortugueseLanguage
{
    public const string Culture = "pt";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' é um endereço de email inválido.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' deve ser superior ou igual a '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' deve ser superior a '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' deve ser inferior ou igual a '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' deve ser inferior a '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' deve ser definido.",
        ValidationErrorCodes.NotNull => "'{0}' não pode ser nulo.",
        ValidationErrorCodes.InvalidFormat => "'{0}' não se encontra no formato correcto.",
        ValidationErrorCodes.Equal => "'{0}' deve ser igual a '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' deve estar entre {1} e {2} (exclusivo). Introduziu {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' deve estar entre {1} e {2}. Introduziu {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' não é um número de cartão de crédito válido.",
        ValidationErrorCodes.ScalePrecision => "'{0}' pode não ser mais do que dígitos {1} no total, com permissão para decimais de {2}. {3} dígitos e {4} decimais foram encontrados.",
        ValidationErrorCodes.LengthBetween => "'{0}' deve ter {1} a {2} caracteres.",
        ValidationErrorCodes.MinLength => "'{0}' deve ser maior ou igual a caracteres {1}.",
        ValidationErrorCodes.MaxLength => "'{0}' deve ser menor ou igual a caracteres {1}.",
        ValidationErrorCodes.ExactLength => "'{0}' deve ter o comprimento de {1} caracteres.",
        _ => null,
    };
}
