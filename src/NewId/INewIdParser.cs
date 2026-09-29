// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Parses a string representation into a <see cref="NewId"/>.</summary>
public interface INewIdParser
{
    /// <summary>Parses the specified text into a <see cref="NewId"/>.</summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed identifier.</returns>
    NewId Parse(in string text);
}
