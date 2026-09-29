#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class AlbanianLanguage
{
    public const string Culture = "sq";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' nuk është një adresë e saktë emaili.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' duhet të jetë më e madhe se ose e barabartë me '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' duhet të jetë më e madhe se '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}'  duhet të jetë më e vogël ose e barabartë me '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' duhet të jetë më e vogël se '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' nuk duhet të jetë bosh.",
        ValidationErrorCodes.NotNull => "'{0}' nuk duhet të jetë bosh.",
        ValidationErrorCodes.InvalidFormat => "'{0}' nuk është në formatin e duhur.",
        ValidationErrorCodes.Equal => "'{0}' duhet të jetë e barabartë me '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' duhet të jetë midis {1} dhe {2}. Ju keni shkruar {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' duhet të jetë midis {1} dhe {2} (përjashtuese). Ju keni shkruar {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' nuk është nje numër i vlefshëm karte krediti.",
        ValidationErrorCodes.ScalePrecision => "'{0}' nuk mund të jetë më shumë se {1} shifra në total, me hapësirë për {2} shifra dhjetore. {3} shifra dhe {4} shifra dhjetore u gjetën.",
        ValidationErrorCodes.LengthBetween => "'{0}' duhet të jetë midis {1} dhe {2} karakteresh.",
        ValidationErrorCodes.MinLength => "Gjatësia e '{0}' duhet të jetë të paktën {1} karaktere.",
        ValidationErrorCodes.MaxLength => "Gjatësia e '{0}' duhet të jetë {1} karaktere ose më pak.",
        ValidationErrorCodes.ExactLength => "'{0}' duhet të jetë {1} karaktere në gjatësi.",
        _ => null,
    };
}
