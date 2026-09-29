// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Diagnostics;

namespace Continuum;

/// <summary>High-resolution tick provider that offsets a <see cref="System.Diagnostics.Stopwatch"/> from the UTC time captured at construction.</summary>
public class StopwatchTickProvider : ITickProvider
{
    readonly DateTime _start;
    readonly Stopwatch _stopwatch;

    /// <summary>Initializes a new instance and starts the underlying stopwatch.</summary>
    public StopwatchTickProvider()
    {
        _start = DateTime.UtcNow;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <inheritdoc />
    public long Ticks => _start.AddTicks(_stopwatch.Elapsed.Ticks).Ticks;
}
