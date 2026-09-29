#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class CatalanLanguage
{
    public const string Culture = "ca";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' no és una adreça de correu electrònic vàlida.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' ha de ser més gran o igual que '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' ha de ser més gran que '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' ha de ser menor o igual que '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' ha de ser menor que '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' no hauria d'estar buit.",
        ValidationErrorCodes.NotNull => "'{0}' no ha d'estar buit.",
        ValidationErrorCodes.InvalidFormat => "'{0}' no té el format correcte.",
        ValidationErrorCodes.Equal => "'{0}' hauria de ser igual a '{1}'.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' ha d'estar entre {1} i {2} (exclusiu). Actualment té un valor de {3}.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' ha d'estar entre {1} i {2}. Actualment té un valor de {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' no és un número de targeta de crèdit vàlid.",
        ValidationErrorCodes.ScalePrecision => "'{0}' no ha de tenir més de {1} dígits en total, amb marge per {2} decimals. S'han trobat {3} i {4} decimals.",
        ValidationErrorCodes.LengthBetween => "'{0}' ha de tenir entre {1} i {2} caràcters.",
        ValidationErrorCodes.MinLength => "'{0}' ha de ser més gran o igual que {1} caràcters.",
        ValidationErrorCodes.MaxLength => "'{0}' ha de ser menor o igual que {1} caràcters.",
        ValidationErrorCodes.ExactLength => "'{0}' ha de tenir una longitud de {1} caràcters.",
        _ => null,
    };
}
