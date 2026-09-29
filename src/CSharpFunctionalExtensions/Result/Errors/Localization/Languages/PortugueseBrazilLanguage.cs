#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class PortugueseBrazilLanguage
{
    public const string Culture = "pt-BR";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' é um endereço de email inválido.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' deve ser superior ou igual a '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' deve ser superior a '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' deve ser inferior ou igual a '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' deve ser inferior a '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' deve ser informado.",
        ValidationErrorCodes.NotNull => "'{0}' não pode ser nulo.",
        ValidationErrorCodes.InvalidFormat => "'{0}' não está no formato correto.",
        ValidationErrorCodes.Equal => "'{0}' deve ser igual a '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' deve, exclusivamente, estar entre {1} e {2}. Você digitou {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' deve estar entre {1} e {2}. Você digitou {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' não é um número válido de cartão de crédito.",
        ValidationErrorCodes.ScalePrecision => "'{0}' não pode ter mais do que {1} dígitos no total, com {2} dígitos decimais. {3} dígitos e {4} decimais foram informados.",
        ValidationErrorCodes.LengthBetween => "'{0}' deve ter entre {1} e {2} caracteres.",
        ValidationErrorCodes.MinLength => "'{0}' deve ser maior ou igual a {1} caracteres.",
        ValidationErrorCodes.MaxLength => "'{0}' deve ser menor ou igual a {1} caracteres.",
        ValidationErrorCodes.ExactLength => "'{0}' deve ter {1} caracteres de comprimento.",
        _ => null,
    };
}
