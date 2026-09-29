#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class AzerbaijaneseLanguage
{
    public const string Culture = "az";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}'  keçərli bir e-poçt ünvanı deyil.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' dəyəri '{1}' dəyərindən böyük və ya bərabər olmalıdır.",
        ValidationErrorCodes.GreaterThan => "'{0}' dəyəri '{1}' dəyərindən böyük olmalıdır.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}', '{1}' dəyərindən kiçik və ya bərabər olmalıdır.",
        ValidationErrorCodes.LessThan => "'{0}', '{1}' dəyərindən kiçik olmalıdır.",
        ValidationErrorCodes.NotEmpty => "'{0}' boş olmamalıdır.",
        ValidationErrorCodes.NotNull => "'{0}' daxil edilməlidir.",
        ValidationErrorCodes.InvalidFormat => "'{0}' dəyərinin formatı düzgün değil.",
        ValidationErrorCodes.Equal => "'{0}', '{1}' dəyərinə bərabər olmalıdır.",
        ValidationErrorCodes.InclusiveBetween => "'{0}', {1} və {2} aralığında olmalıdır. {3} dəyərini daxil etmisiniz.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}', {1} (daxil deyil) və {2} (daxil deyil) aralığında olmalıdır. {3} dəyərini daxil etmisiniz.",
        ValidationErrorCodes.CreditCard => "'{0}' keçərli kredit kartı nömrəsi değil.",
        ValidationErrorCodes.ScalePrecision => "'{0}' icazə verilən {2} rəqəmli onluq hissə ilə birlikdə ümumilikdə {1} rəqəmdən ibarət olmalıdır. {3} tam və {4} onluq ədəd tapıldı.",
        ValidationErrorCodes.LengthBetween => "'{0}', {1} və {2} aralığında simvol uzunluğunda olmalıdır.",
        ValidationErrorCodes.MinLength => "'{0}', {1} simvoldan böyük və ya bərabər olmalıdır.",
        ValidationErrorCodes.MaxLength => "'{0}', {1} simvoldan kiçik və ya bərabər olmalıdır.",
        ValidationErrorCodes.ExactLength => "'{0}', {1} simvol uzunluğunda olmalıdır.",
        _ => null,
    };
}
