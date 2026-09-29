// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Extension methods for converting between <see cref="Guid"/> and <see cref="NewId"/>.</summary>
public static class NewIdExtensions
{
    /// <summary>Converts a <see cref="Guid"/> in MSSQL Server ordered format to a <see cref="NewId"/>.</summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>The equivalent <see cref="NewId"/>.</returns>
    public static NewId ToNewId(this Guid guid)
    {
        return NewId.FromGuid(guid);
    }

    /// <summary>Converts a <see cref="Guid"/> in sequential (lexicographical) format to a <see cref="NewId"/>.</summary>
    /// <param name="guid">The <see cref="Guid"/> to convert.</param>
    /// <returns>The equivalent <see cref="NewId"/>.</returns>
    public static NewId ToNewIdFromSequential(this Guid guid)
    {
        return NewId.FromSequentialGuid(guid);
    }
}
