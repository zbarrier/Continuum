#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class KazakhLanguage
{
    public const string Culture = "kk";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' Қате электрондық пошта мекенжайы.",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' мән мынадан -'{1}' үлкен немесе оған тең болуы керек.",
        ValidationErrorCodes.GreaterThan => "'{0}' мән мынадан -'{1}' үлкенірек болуы керек.",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' мән мынадан -'{1}' кіші немесе оған тең болуы керек.",
        ValidationErrorCodes.LessThan => "'{0}' мән мынадан -'{1}' аз болуы керек.",
        ValidationErrorCodes.NotEmpty => "'{0}' толтырылуы керек.",
        ValidationErrorCodes.NotNull => "'{0}' толтырылуы керек.",
        ValidationErrorCodes.InvalidFormat => "'{0}' дұрыс пішімде емес.",
        ValidationErrorCodes.Equal => "'{0}' '{1}' мәніне тең болуы керек.",
        ValidationErrorCodes.InclusiveBetween => "'{0}' {1} және {2} аралығында болуы керек. Енгізілген мән: {3}.",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' {1} және {2} аралығында болуы керек (осы мәндерді қоспағанда). Енгізілген мән: {3}.",
        ValidationErrorCodes.CreditCard => "'{0}' қате карта нөмірі.",
        ValidationErrorCodes.ScalePrecision => "Жалпы алғанда, '{0}' {1} цифрдан аспауы, оның ішінде {2} ондық таңба. Енгізілген мән бүтін бөліктегі {3} цифрдан тұрады және {4} ондық белгі.",
        ValidationErrorCodes.LengthBetween => "'{0}' таңбаларының саны {1} және {2} арасында болуы керек.",
        ValidationErrorCodes.MinLength => "'{0}' таңбаларының саны мынадан - {1} кем емес болуы керек.",
        ValidationErrorCodes.MaxLength => "'{0}' таңбаларының саны мынадан - {1} көп емес болуы керек.",
        ValidationErrorCodes.ExactLength => "'{0}' ұзындығы {1} таңба болуы керек.",
        _ => null,
    };
}
