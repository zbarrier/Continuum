// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Continuum;

/// <summary>Formats a <see cref="NewId"/> as 32 hexadecimal digits without separators.</summary>
public class HexFormatter : INewIdFormatter
{
    readonly uint _alpha;
    const uint LowerCaseUInt = 0x2020U;

    /// <summary>Initializes a new instance of the <see cref="HexFormatter"/> class.</summary>
    /// <param name="upperCase"><see langword="true"/> to emit upper-case hexadecimal digits.</param>
    public HexFormatter(bool upperCase = false)
    {
        _alpha = upperCase ? 0 : LowerCaseUInt;
    }

    /// <inheritdoc />
    public unsafe string Format(in byte[] bytes)
    {
        Debug.Assert(bytes.Length == 16);

        if (Avx2.IsSupported && BitConverter.IsLittleEndian)
        {
            var isUpperCase = _alpha != LowerCaseUInt;
            return string.Create(32, (bytes, isUpperCase), (span, state) =>
            {
                var (bytes, isUpper) = state;

                var inputVec = MemoryMarshal.Read<Vector128<byte>>(bytes);
                var hexVec = IntrinsicsHelper.EncodeBytesHex(inputVec, isUpper);

                var byteSpan = MemoryMarshal.Cast<char, byte>(span);
                IntrinsicsHelper.Vector256ToCharUtf16(hexVec, byteSpan);
            });
        }

        var result = stackalloc char[32];

        for (int pos = 0; pos < bytes.Length; pos++)
        {
            HexToChar(bytes[pos], result, pos * 2, _alpha);
        }

        return new string(result, 0, 32);
    }

    // From https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/HexConverter.cs#L83
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static unsafe void HexToChar(byte value, char* buffer, int startingIndex, uint casing)
    {
        uint difference = (((uint)value & 0xF0U) << 4) + ((uint)value & 0x0FU) - 0x8989U;
        uint packedResult = ((((uint)(-(int)difference) & 0x7070U) >> 4) + difference + 0xB9B9U) | (uint)casing;

        buffer[startingIndex + 1] = (char)(packedResult & 0xFF);
        buffer[startingIndex] = (char)(packedResult >> 8);
    }
}
