#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class HindiLanguage
{
    public const string Culture = "hi";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' मान्य ईमेल एड्रेस नहीं है।",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' '{1}' से अधिक या के उसके बराबर होनी चाहिए।",
        ValidationErrorCodes.GreaterThan => "'{0}' '{1}' से अधिक होनी चाहिए।",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' '{1}' से कम या के उसके बराबर होनी चाहिए।",
        ValidationErrorCodes.LessThan => "'{0}' '{1}' से कम होनी चाहिए।",
        ValidationErrorCodes.NotEmpty => "'{0}' खाली नहीं होना चाहिए।",
        ValidationErrorCodes.NotNull => "'{0}' खाली नहीं होना चाहिए।",
        ValidationErrorCodes.InvalidFormat => "'{0}' सही प्रारूप में नहीं है।",
        ValidationErrorCodes.Equal => "'{0}' '{1}' से बराबर होना चाहिए।",
        ValidationErrorCodes.InclusiveBetween => "'{0}' {1} और {2} के बीच में होनी चाहिए।. आपने {3} दर्ज किया है।",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' {1} और {2} (अनन्य) के बीच में होनी चाहिए।. आपने {3} दर्ज किया है।",
        ValidationErrorCodes.CreditCard => "'{0}' मान्य क्रेडिट कार्ड नंबर नहीं है।",
        ValidationErrorCodes.ScalePrecision => "'{0}' कुल में {1} अंकों से अधिक नहीं हो सकता है, {2} दशमलव के के साथ।. {3} अंक और {4} दशमलव पाए गए है।",
        ValidationErrorCodes.LengthBetween => "'{0}' {1} और {2} अक्षरों के बीच होना चाहिए।",
        ValidationErrorCodes.MinLength => "'{0}' {1} वर्णों से अधिक या उसके बराबर होना चाहिए।",
        ValidationErrorCodes.MaxLength => "'{0}' {1} वर्णों से कम या उसके बराबर होना चाहिए।",
        ValidationErrorCodes.ExactLength => "'{0}' {1} अक्षरों के उसके बराबर होनी चाहिए।",
        _ => null,
    };
}
