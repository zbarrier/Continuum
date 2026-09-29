// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Parses a 26-character Base32 string into a <see cref="NewId"/>.</summary>
public class Base32Parser : INewIdParser
{
    const string ConvertChars = "abcdefghijklmnopqrstuvwxyz234567ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
    const string HexChars = "0123456789ABCDEF";
    const string InvalidInputString = "The input string contains invalid characters";

    static readonly ThreadLocal<char[]> _buffer = new ThreadLocal<char[]>(() => new char[32]);
    readonly string _chars;

    /// <summary>Initializes a new instance using the default Base32 alphabet (upper and lower case).</summary>
    public Base32Parser()
        :
        this(ConvertChars)
    {
    }

    /// <summary>Initializes a new instance using a custom alphabet.</summary>
    /// <param name="chars">The alphabet; its length must be a multiple of 32 (each block of 32 characters is an accepted alias set).</param>
    public Base32Parser(in string chars)
    {
        if (chars.Length % 32 != 0)
            throw new ArgumentException("The characters must be a multiple of 32", nameof(chars));

        _chars = chars;
    }

    /// <inheritdoc />
    public NewId Parse(in string text)
    {
        if (text.Length != 26)
            throw new ArgumentException("The input string must be 26 characters", nameof(text));

        var buffer = _buffer.Value!;

        var bufferOffset = 0;
        var offset = 0;
        long number;
        for (var i = 0; i < 6; ++i)
        {
            number = 0;
            for (var j = 0; j < 4; j++)
            {
                var index = _chars.IndexOf(text[offset + j]);
                if (index < 0)
                    throw new ArgumentException(InvalidInputString);

                number = number * 32 + index % 32;
            }

            ConvertLongToBase16(buffer, bufferOffset, number, 5);

            offset += 4;
            bufferOffset += 5;
        }

        number = 0;
        for (var j = 0; j < 2; j++)
        {
            var index = _chars.IndexOf(text[offset + j]);
            if (index < 0)
                throw new ArgumentException(InvalidInputString);

            number = number * 32 + index % 32;
        }

        ConvertLongToBase16(buffer, bufferOffset, number, 2);

        return new NewId(new string(buffer, 0, 32));
    }

    static void ConvertLongToBase16(in char[] buffer, int offset, long value, int count)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            var index = (int)(value % 16);
            buffer[offset + i] = HexChars[index];
            value /= 16;
        }
    }
}
