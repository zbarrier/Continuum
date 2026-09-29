// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Tick provider backed by <see cref="DateTime.UtcNow"/>.</summary>
public class DateTimeTickProvider : ITickProvider
{
    /// <inheritdoc />
    public long Ticks => DateTime.UtcNow.Ticks;
}
