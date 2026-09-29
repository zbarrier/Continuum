// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Generates <see cref="NewId"/> values.</summary>
public interface INewIdGenerator
{
    /// <summary>Generates the next <see cref="NewId"/>.</summary>
    /// <returns>A new, time-ordered identifier.</returns>
    NewId Next();

    /// <summary>Fills a section of an existing array with newly generated identifiers.</summary>
    /// <param name="ids">The array to fill.</param>
    /// <param name="index">The starting offset within <paramref name="ids"/>.</param>
    /// <param name="count">The number of identifiers to generate.</param>
    /// <returns>A segment over the generated identifiers.</returns>
    ArraySegment<NewId> Next(NewId[] ids, int index, int count);
    /// <summary>Generates the next identifier as a <see cref="Guid"/> in the MSSQL Server ordered format.</summary>
    /// <returns>An MSSQL Server ordered <see cref="Guid"/>.</returns>
    Guid NextGuid();
    /// <summary>Generates the next identifier as a <see cref="Guid"/> in sequential (lexicographical) order.</summary>
    /// <returns>A lexicographically ordered <see cref="Guid"/>.</returns>
    Guid NextSequentialGuid();
}
