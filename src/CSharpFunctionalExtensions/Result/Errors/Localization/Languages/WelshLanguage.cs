#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class WelshLanguage
{
    public const string Culture = "cy";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "Nid yw '{0}' yn gyfeiriad e-bost dilys.",
        ValidationErrorCodes.GreaterThanOrEqual => "Rhaid i '{0}' fod yn fwy na '{1}', neu'n gyfartal ag o.",
        ValidationErrorCodes.GreaterThan => "Rhaid i '{0}' fod yn fwy na '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Rhaid i '{0}' fod yn llai na '{1}', neu'n gyfartal ag o.",
        ValidationErrorCodes.LessThan => "Rhaid i '{0}' fod yn llai na '{1}'.",
        ValidationErrorCodes.NotEmpty => "Ni ddylai '{0}' fod yn wag.",
        ValidationErrorCodes.NotNull => "Ni ddylai '{0}' fod yn wag.",
        ValidationErrorCodes.InvalidFormat => "Nid yw '{0}' yn y fformat cywir.",
        ValidationErrorCodes.Equal => "Mae'n rhaid i '{0}' fod yn gyfartal â '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Rhaid i '{0}' fod rhwng {1} a {2}. Rydych wedi rhoi {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Rhaid i '{0}' fod rhwng {1} a {2} (ddim yn gynwysedig). Rydych wedi rhoi {3}.",
        ValidationErrorCodes.CreditCard => "Nid yw '{0}' yn rhif cerdyn credyd dilys.",
        ValidationErrorCodes.ScalePrecision => "Ni ddylai '{0}' fod yn fwy na {1} digid i gyd  gan ganiatáu ar gyfer {2} degolyn. Canfuwyd {3} digid a {4} degolyn.",
        ValidationErrorCodes.LengthBetween => "Rhaid i '{0}' fod rhwng {1} a {2} o nodau.",
        ValidationErrorCodes.MinLength => "Rhaid i '{0}' fod o leiaf {1} nod o hyd.",
        ValidationErrorCodes.MaxLength => "Rhaid i '{0}' fod yn {1} nod o hyd neu lai.",
        ValidationErrorCodes.ExactLength => "Mae'n rhaid i '{0}' fod yn {1} nod o hyd.",
        _ => null,
    };
}
