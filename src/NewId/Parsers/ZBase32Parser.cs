// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Parses a 26-character z-base-32 string into a <see cref="NewId"/>.</summary>
public class ZBase32Parser : Base32Parser
{
    const string ConvertChars = "ybndrfg8ejkmcpqxot1uwisza345h769YBNDRFG8EJKMCPQXOT1UWISZA345H769";

    const string TransposeChars = "ybndrfg8ejkmcpqx0tlvwis2a345h769YBNDRFG8EJKMCPQX0TLVWIS2A345H769";

    /// <summary>Initializes a new instance of the <see cref="ZBase32Parser"/> class.</summary>
    /// <param name="handleTransposedCharacters"><see langword="true"/> to also accept commonly confused characters (0/o, l/1, v/u, 2/z).</param>
    public ZBase32Parser(bool handleTransposedCharacters = false)
        : base(handleTransposedCharacters ? ConvertChars + TransposeChars : ConvertChars)
    {
    }
}
