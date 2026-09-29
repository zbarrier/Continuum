#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class JapaneseLanguage
{
    public const string Culture = "ja";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}' は有効なメールアドレスではありません。",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}' は '{1}' 以上でなければなりません。",
        ValidationErrorCodes.GreaterThan => "'{0}' は '{1}' よりも大きくなければなりません。",
        ValidationErrorCodes.LessThanOrEqual => "'{0}' は '{1}' 以下である必要があります。",
        ValidationErrorCodes.LessThan => "'{0}' は '{1}' 未満である必要があります。",
        ValidationErrorCodes.NotEmpty => "'{0}' は空であってはなりません。",
        ValidationErrorCodes.NotNull => "'{0}' は空であってはなりません。",
        ValidationErrorCodes.InvalidFormat => "'{0}' は正しい形式ではありません。",
        ValidationErrorCodes.Equal => "'{0}' は '{1}' と等しくなくてはなりません。",
        ValidationErrorCodes.InclusiveBetween => "'{0}' は {1} から {2} までの間でなければなりません。 {3} と入力されています。",
        ValidationErrorCodes.ExclusiveBetween => "'{0}' は {1} と {2} の間でなければなりません。 {3} と入力されています。",
        ValidationErrorCodes.CreditCard => "'{0}' は有効なクレジットカード番号ではありません。",
        ValidationErrorCodes.ScalePrecision => "'{0}' は合計で {1} 桁、小数点以下は{2} 桁を超えてはなりません。 {3} 桁、小数点以下は{4} で入力されています。",
        ValidationErrorCodes.LengthBetween => "'{0}' は {1} から {2} 文字の間で入力する必要があります。",
        ValidationErrorCodes.MinLength => "'{0}' は少なくとも {1} 文字を入力しなければなりません。",
        ValidationErrorCodes.MaxLength => "'{0}' は {1} 文字以下でなければなりません。",
        ValidationErrorCodes.ExactLength => "'{0}' は {1} 文字でなくてはなりません。",
        _ => null,
    };
}
