#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class GreekLanguage
{
    public const string Culture = "el";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "Το πεδίο '{0}' δεν περιέχει μια έγκυρη διεύθυνση email.",
        ValidationErrorCodes.GreaterThanOrEqual => "Το πεδίο '{0}' πρέπει να έχει τιμή μεγαλύτερη ή ίση με '{1}'.",
        ValidationErrorCodes.GreaterThan => "Το πεδίο '{0}' πρέπει να έχει τιμή μεγαλύτερη από '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "Το πεδίο '{0}' πρέπει να έχει τιμή μικρότερη ή ίση με '{1}'.",
        ValidationErrorCodes.LessThan => "Το πεδίο '{0}' πρέπει να έχει τιμή μικρότερη από '{1}'.",
        ValidationErrorCodes.NotEmpty => "Το πεδίο '{0}' δεν πρέπει να είναι κενό.",
        ValidationErrorCodes.NotNull => "Το πεδίο '{0}' δεν πρέπει να είναι κενό.",
        ValidationErrorCodes.InvalidFormat => "Η τιμή του πεδίου '{0}' δεν έχει αποδεκτή μορφή.",
        ValidationErrorCodes.Equal => "Το πεδίο '{0}' πρέπει να έχει τιμή ίση με '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "Το πεδίο '{0}' πρέπει να έχει τιμή μεταξύ {1} και {2}. Καταχωρίσατε την τιμή {3}.",
        ValidationErrorCodes.ExclusiveBetween => "Το πεδίο '{0}' πρέπει να έχει τιμή μεγαλύτερη από {1} και μικρότερη από {2}. Καταχωρίσατε την τιμή  {3}.",
        ValidationErrorCodes.CreditCard => "Το πεδίο '{0}' δεν περιέχει αποδεκτό αριθμό πιστωτικής κάρτας.",
        ValidationErrorCodes.ScalePrecision => "'Το πεδίο '{0}' δεν μπορεί να έχει περισσότερα από {1} ψηφία στο σύνολο, με μέγιστο επιτρεπόμενο αριθμό δεκαδικών τα {2} ψηφία. Έχετε καταχωρίσει {3} ψηφία συνολικά με {4} δεκαδικά.",
        ValidationErrorCodes.LengthBetween => "Το πεδίο '{0}' πρέπει να έχει μήκος μεταξύ {1} και {2} χαρακτήρες.",
        ValidationErrorCodes.MinLength => "Το μήκος του πεδίου '{0}' πρέπει να είναι τουλάχιστον {1} χαρακτήρες.",
        ValidationErrorCodes.MaxLength => "Το μήκος του πεδίου '{0}' πρέπει να είναι το πολύ {1} χαρακτήρες.",
        ValidationErrorCodes.ExactLength => "Το πεδίο '{0}' πρέπει να έχει μήκος ίσο με {1} χαρακτήρες.",
        _ => null,
    };
}
