#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class RomanianLanguage
{
    public const string Culture = "ro";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nu este o adresă de email validă.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' trebuie să fie mai mare sau egală cu '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' trebuie să fie mai mare ca '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' trebuie să fie mai mică sau egală cu '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' trebuie să fie mai mică decât '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' nu ar trebui să fie goală.",
        ValidationErrorCodes.NotNull => "'{0}' nu trebui să fie goală.",
        ValidationErrorCodes.InvalidFormat => "'{0}' nu este în formatul corect.",
        ValidationErrorCodes.Equal => "'{0}' ar trebui să fie egal cu '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' trebuie sa fie între {1} şi {2}. Ai introdus {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' trebuie sa fie între {1} şi {2} (exclusiv). Ai introdus {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nu este un număr de card de credit valid.",
        ValidationErrorCodes.ScalePrecision => "'{0}' nu poate fi mai mare decât {1} de cifre în total, cu alocație pentru {2} zecimale. {3} cifre şi {4} au fost găsite zecimale.",
        ValidationErrorCodes.LengthBetween => "'{0}' trebuie să fie între {1} şi {2} caractere.",
        ValidationErrorCodes.MinLength => "'{0}' trebuie să fie mai mare sau egală cu caracterele {1}.",
        ValidationErrorCodes.MaxLength => "'{0}' trebuie să fie mai mică sau egală cu caracterele {1}.",
        ValidationErrorCodes.ExactLength => "'{0}' trebui să aibe lungimea maximă {1} de caractere.",
        _ => null,
    };
}
