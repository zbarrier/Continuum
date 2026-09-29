#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class SlovenianLanguage
{
    public const string Culture = "sl";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ni veljaven e-poštni naslov.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' mora biti večji ali enak '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' mora biti večji od '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' mora biti manjši ali enak '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' mora biti manjši od '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' ne sme biti prazen.",
        ValidationErrorCodes.NotNull => "'{0}' ne sme biti prazen.",
        ValidationErrorCodes.InvalidFormat => "'{0}' ni v pravilni obliki.",
        ValidationErrorCodes.Equal => "'{0}' mora biti enak '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' mora biti med {1} in {2}. Vnesli ste {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' mora biti med {1} in {2} (izključno). Vnesli ste {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' ni veljavna številka kreditne kartice.",
        ValidationErrorCodes.ScalePrecision => "'{0}' ne sme biti več kot {1} natančno števk in {2} decimalk. Vnesli ste {3} števk in {4} decimalk.",
        ValidationErrorCodes.LengthBetween => "'{0}' imeti dolžino med {1} in {2} znakov. ",
        ValidationErrorCodes.MinLength => "'{0}' mora imeti dolžino večjo ali enako {1}.",
        ValidationErrorCodes.MaxLength => "'{0}' mora imeti dolžino manjšo ali enako {1}.",
        ValidationErrorCodes.ExactLength => "'{0}' mora imeti {1} znakov.",
        _ => null,
    };
}
