#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class BengaliLanguage
{
    public const string Culture = "bn";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' বৈধ ইমেইল ঠিকানা নয়।",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' অবশ্যই '{1}' এর সমান অথবা বেশি হবে।",
        ValidationErrorCodes.GreaterThan => "'{0}' অবশ্যই '{1}' এর বেশি হবে।",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' অবশ্যই '{1}' এর সমান অথবা কম হবে।",
        ValidationErrorCodes.LessThan => "'{0}' অবশ্যই '{1}' এর চেয়ে কম হবে।",
        ValidationErrorCodes.NotEmpty => "'{0}' খালি হতে পারবে না।",
        ValidationErrorCodes.NotNull => "'{0}' খালি হতে পারবে না।",
        ValidationErrorCodes.InvalidFormat => "'{0}' সঠিক বিন্যাসে নেই।",
        ValidationErrorCodes.Equal => "'{0}' অবশ্যই '{1}' এর সমান হবে।",
        ValidationErrorCodes.InclusiveBetween => "'{0}' অবশ্যই {1} থেকে {2} এর মধ্যে হবে। আপনি {3} প্রদান করেছেন।",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' অবশ্যই {1} থেকে {2} এর বাহিরে হবে না। আপনি {3} প্রদান করেছেন।",
        ValidationErrorCodes.CreditCard => "'{0}' বৈধ ক্রেডিট কার্ড সংখ্যা নয়।",
        ValidationErrorCodes.ScalePrecision => "'{0}' মোট {1} অঙ্কের বেশি হবে না। {2} বৈধ দশমাংশ, কিন্তু প্রদত্ত {3} সংখ্যাটি {4} দশমাংশের",
        ValidationErrorCodes.LengthBetween => "'{0}' এর অক্ষর সংখ্যা অবশ্যই {1} থেকে {2} এর মধ্যে হবে।",
        ValidationErrorCodes.MinLength => "'{0}' এর অক্ষর সংখ্যা কমপক্ষে {1} অথবা এর চেয়ে বেশি হবে।",
        ValidationErrorCodes.MaxLength => "'{0}' এর অক্ষর সংখ্যা সর্বোচ্চ {1}টি অথবা এর চেয়ে কম হবে।",
        ValidationErrorCodes.ExactLength => "'{0}' এর অক্ষর সংখ্যা অবশ্যই {1}টি হবে।",
        _ => null,
    };
}
