#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class TeluguLanguage
{
    public const string Culture = "te";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' చెల్లుబాటు అయ్యే ఈమెయిల్ చిరునామా కాదు.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' తప్పనిసరిగా '{1}' కంటే ఎక్కువ లేదా సమానం అయ్యి ఉండాలి.",
        ValidationErrorCodes.GreaterThan => "'{0}' తప్పనిసరిగా '{1}' కంటే ఎక్కువ అయ్యి ఉండాలి.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' తప్పనిసరిగా '{1}' కంటే తక్కువ లేదా సమానం అయ్యి ఉండాలి.",
        ValidationErrorCodes.LessThan => "'{0}' తప్పనిసరిగా '{1}' కంటే తక్కువ అయ్యి ఉండాలి.",
        ValidationErrorCodes.NotEmpty => "'{0}' ఖాళీగా ఉండకూడదు.",
        ValidationErrorCodes.NotNull => "'{0}' ఖాళీగా ఉండకూడదు.",
        ValidationErrorCodes.InvalidFormat => "'{0}' సరైన ఫార్మాట్‌లో లేదు.",
        ValidationErrorCodes.Equal => "'{0}' తప్పనిసరిగా '{1}' కి సమానం అయ్యి ఉండాలి.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' తప్పనిసరిగా {1} మరియు {2} మధ్య ఉండాలి. మీరు {3} నమోదు చేశారు.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' తప్పనిసరిగా {1} మరియు {2} మధ్య (ఎక్స్‌క్లూజివ్) ఉండాలి. మీరు {3} నమోదు చేశారు.",
        ValidationErrorCodes.CreditCard => "'{0}' చెల్లుబాటు అయ్యే క్రెడిట్ కార్డ్ నంబర్ కాదు.",
        ValidationErrorCodes.ScalePrecision => "'{0}' మొత్తం {1} అంకెలకు మించి ఉండకూడదు, {2} దశాంశాలకు అనుమతి ఉంది. {3} అంకెలు మరియు {4} దశాంశాలు కనుగొనబడ్డాయి.",
        ValidationErrorCodes.LengthBetween => "'{0}' {1} మరియు {2} అక్షరాల మధ్య ఉండాలి.",
        ValidationErrorCodes.MinLength => "'{0}' యొక్క పొడవు కనీసం {1} అక్షరాలు ఉండాలి.",
        ValidationErrorCodes.MaxLength => "'{0}' యొక్క పొడవు {1} అక్షరాలు లేదా అంతకంటే తక్కువ ఉండాలి.",
        ValidationErrorCodes.ExactLength => "'{0}' తప్పనిసరిగా {1} అక్షరాల పొడవు ఉండాలి.",
        _ => null,
    };
}
