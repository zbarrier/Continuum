// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).
//           Converted from ITickProvider to a TimeProvider subclass.

namespace Continuum;

/// <summary>A <see cref="TimeProvider"/> whose UTC time never moves backward: it adds elapsed high-resolution time to the UTC time captured at construction.</summary>
public class StopwatchTimeProvider : TimeProvider
{
    readonly DateTimeOffset _start;
    readonly long _startTimestamp;

    /// <summary>Initializes a new instance, capturing the current UTC time and high-resolution timestamp.</summary>
    public StopwatchTimeProvider()
    {
        _start = System.GetUtcNow();
        _startTimestamp = GetTimestamp();
    }

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow() => _start + GetElapsedTime(_startTimestamp);
}