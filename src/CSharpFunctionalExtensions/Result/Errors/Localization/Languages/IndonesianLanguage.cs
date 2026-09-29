#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class IndonesianLanguage
{
    public const string Culture = "id";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' bukan alamat email yang benar.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' harus lebih besar dari atau sama dengan '{1}'.",
        ValidationErrorCodes.GreaterThan => "'{0}' harus lebih besar dari '{1}'.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' harus kurang dari atau sama dengan '{1}'.",
        ValidationErrorCodes.LessThan => "'{0}' harus kurang dari '{1}'.",
        ValidationErrorCodes.NotEmpty => "'{0}' tidak boleh kosong.",
        ValidationErrorCodes.NotNull => "'{0}' tidak boleh kosong.",
        ValidationErrorCodes.InvalidFormat => "'{0}' bukan dalam format yang benar.",
        ValidationErrorCodes.Equal => "'{0}' harus sama dengan '{1}'.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' harus di antara {1} dan {2}. Anda memasukkan {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' harus di antara {1} dan {2} (exclusive). Anda memasukkan {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' bukan nomor kartu kredit yang benar.",
        ValidationErrorCodes.ScalePrecision => "Jumlah digit '{0}' tidak boleh lebih dari {1}, dengan toleransi {2} desimal. {3} digit dan {4} desimal ditemukan.",
        ValidationErrorCodes.LengthBetween => "'{0}' harus di antara {1} dan {2} karakter.",
        ValidationErrorCodes.MinLength => "Panjang dari '{0}' harus paling tidak {1} karakter.",
        ValidationErrorCodes.MaxLength => "Panjang dari '{0}' harus {1} karakter atau fewer.",
        ValidationErrorCodes.ExactLength => "'{0}' harus {1} karakter panjangnya.",
        _ => null,
    };
}
