#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class TamilLanguage
{
    public const string Culture = "ta";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' ஒரு செல்லுபடியான மின்னஞ்சல் முகவரி அல்ல.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' '{1}'-ஐ விட அதிகமாக அல்லது சமமாக இருக்க வேண்டும்.",
        ValidationErrorCodes.GreaterThan => "'{0}' '{1}'-ஐ விட அதிகமாக இருக்க வேண்டும்.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' '{1}'-ஐ விட குறைவாக அல்லது சமமாக இருக்க வேண்டும்.",
        ValidationErrorCodes.LessThan => "'{0}' '{1}'-ஐ விட குறைவாக இருக்க வேண்டும்.",
        ValidationErrorCodes.NotEmpty => "'{0}' காலியாக இருக்கக்கூடாது.",
        ValidationErrorCodes.NotNull => "'{0}' காலியாக இருக்கக்கூடாது.",
        ValidationErrorCodes.InvalidFormat => "'{0}' சரியான வடிவத்தில் இல்லை.",
        ValidationErrorCodes.Equal => "'{0}' '{1}'-இற்குச் சமமாக இருக்க வேண்டும்.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' {1} மற்றும் {2} இடையில் இருக்க வேண்டும். நீங்கள் {3} உள்ளீடு செய்துள்ளீர்கள்.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' {1} மற்றும் {2} இடையே (விலக்கப்பட) இருக்க வேண்டும். நீங்கள் {3} உள்ளீடு செய்துள்ளீர்கள்.",
        ValidationErrorCodes.CreditCard => "'{0}' ஒரு செல்லுபடியான கிரெடிட் கார்டு எண் அல்ல.",
        ValidationErrorCodes.ScalePrecision => "'{0}' {1} இலக்கங்களை மொத்தமாக (அதில் {2} தசம இலக்கங்கள்) மீறக்கூடாது. {3} இலக்கங்கள் மற்றும் {4} தசமங்கள் உள்ளன.",
        ValidationErrorCodes.LengthBetween => "'{0}' {1} மற்றும் {2} எழுத்துகளுக்கு இடையில் இருக்க வேண்டும்.",
        ValidationErrorCodes.MinLength => "'{0}' குறைந்தது {1} எழுத்துகள் இருக்க வேண்டும்.",
        ValidationErrorCodes.MaxLength => "'{0}' அதிகபட்சம் {1} எழுத்துகள் இருக்க வேண்டும்.",
        ValidationErrorCodes.ExactLength => "'{0}' {1} எழுத்துகள் நீளமாக இருக்க வேண்டும்.",
        _ => null,
    };
}
