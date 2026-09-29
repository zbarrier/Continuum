#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class SwedishLanguage
{
    public const string Culture = "sv";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "\"{0}\" är inte en giltig e-postadress.",
        ValidationErrorCodes.GreaterThanOrEqual => "\"{0}\" måste vara större än eller lika med {1}.",
        ValidationErrorCodes.GreaterThan => "\"{0}\" måste vara större än {1}.",
        ValidationErrorCodes.LessThanOrEqual => "\"{0}\" måste vara mindre än eller lika med {1}.",
        ValidationErrorCodes.LessThan => "\"{0}\" måste vara mindre än {1}.",
        ValidationErrorCodes.NotEmpty => "\"{0}\" måste anges.",
        ValidationErrorCodes.NotNull => "\"{0}\" måste anges.",
        ValidationErrorCodes.InvalidFormat => "\"{0}\" har inte ett korrekt format.",
        ValidationErrorCodes.Equal => "\"{0}\" måste vara lika med \"{1}\".",
        ValidationErrorCodes.InclusiveBetween => "\"{0}\" måste vara mellan {1} och {2}. Du angav {3}.",
        ValidationErrorCodes.ExclusiveBetween => "\"{0}\" måste vara mellan {1} och {2} (gränsvärdena exkluderade). Du angav {3}.",
        ValidationErrorCodes.CreditCard => "\"{0}\" är inte ett giltigt kreditkortsnummer.",
        ValidationErrorCodes.ScalePrecision => "\"{0}\" får inte vara mer än {1} siffror totalt, med förbehåll för {2} decimaler. {3} siffror och {4} decimaler hittades.",
        ValidationErrorCodes.LengthBetween => "\"{0}\" måste vara mellan {1} och {2} tecken långt.",
        ValidationErrorCodes.MinLength => "\"{0}\" måste vara större än eller lika med {1} tecken.",
        ValidationErrorCodes.MaxLength => "\"{0}\" måste vara mindre än eller lika med {1} tecken.",
        ValidationErrorCodes.ExactLength => "\"{0}\" måste vara {1} tecken långt.",
        _ => null,
    };
}
