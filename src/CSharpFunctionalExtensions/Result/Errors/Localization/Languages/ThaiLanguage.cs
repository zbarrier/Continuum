#nullable enable

// Copyright (c) .NET Foundation and contributors.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Translations adapted from FluentValidation (https://github.com/FluentValidation/FluentValidation).
// Modified: named placeholders converted to positional placeholders and keys remapped to Continuum error codes.

namespace Continuum.CSharpFunctionalExtensions;

internal static class ThaiLanguage
{
    public const string Culture = "th";

    public static string? GetTranslation(string code) => code switch
    {
        ValidationErrorCodes.Email => "'{0}'ไม่ใช่อีเมลที่ถูกต้อง",
        ValidationErrorCodes.GreaterThanOrEqual => "'{0}'ต้องมีค่ามากกว่าหรือเท่ากับ'{1}'",
        ValidationErrorCodes.GreaterThan => "'{0}'ต้องมีค่ามากกว่า'{1}'",
        ValidationErrorCodes.LessThanOrEqual => "'{0}'ต้องมีค่าน้อยกว่าหรือเท่ากับ'{1}'.",
        ValidationErrorCodes.LessThan => "'{0}'ต้องมีค่าน้อยกว่า'{1}'",
        ValidationErrorCodes.NotEmpty => "'{0}'ต้องไม่มีค่าว่างเปล่า",
        ValidationErrorCodes.NotNull => "'{0}'ต้องมีค่า",
        ValidationErrorCodes.InvalidFormat => "ข้อมูลของ'{0}'ผิดรูปแบบ",
        ValidationErrorCodes.Equal => "'{0}'ต้องมีค่าเท่ากับ'{1}'",
        ValidationErrorCodes.InclusiveBetween => "'{0}'ต้องมีค่าระหว่าง{1}ถึง{2} คุณให้ข้อมูล{3}",
        ValidationErrorCodes.ExclusiveBetween => "'{0}'ต้องมีค่าอยู่ระหว่างแต่ไม่รวม{1}และ{2} คุณให้ข้อมูล{3}",
        ValidationErrorCodes.CreditCard => "'{0}'ไม่ใช่ตัวเลขบัตรเครดิตที่ถูกต้อง",
        ValidationErrorCodes.ScalePrecision => "'{0}'ต้องไม่มีจำนวนตัวเลขมากกว่า{1}ตำแหน่งทั้งหมด และมีจุดทศนิยม{2}ตำแหน่ง ข้อมูลมีตัวเลขค่าเต็ม{3}ตำแหน่งและจุดทศนิยม{4}ตำแหน่ง",
        ValidationErrorCodes.LengthBetween => "'{0}'ต้องมีจำนวนตัวอักษรระหว่าง{1}แหละ{2}ตัวอักษร",
        ValidationErrorCodes.MinLength => "จำนวนตัวอักษรของ'{0}'ต้องมีอย่างน้อย{1}ตัวอักษร",
        ValidationErrorCodes.MaxLength => "จำนวนตัวอักษรของ'{0}'ต้องเท่ากับหรือน้อยกว่า{1}ตัวอักษร",
        ValidationErrorCodes.ExactLength => "จำนวนตัวอักษรของ'{0}'ต้องเท่ากับ{1}ตัวอักษร",
        _ => null,
    };
}
