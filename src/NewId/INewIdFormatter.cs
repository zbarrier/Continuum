// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Formats the 16 bytes of a <see cref="NewId"/> into a string representation.</summary>
public interface INewIdFormatter
{
    /// <summary>Formats the specified bytes as a string.</summary>
    /// <param name="bytes">The 16 bytes of the identifier.</param>
    /// <returns>The formatted string.</returns>
    string Format(in byte[] bytes);
}
