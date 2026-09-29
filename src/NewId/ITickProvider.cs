// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Provides the timestamp source used when generating identifiers.</summary>
public interface ITickProvider
{
    /// <summary>Gets the current time, in UTC <see cref="DateTime"/> ticks.</summary>
    long Ticks { get; }
}
