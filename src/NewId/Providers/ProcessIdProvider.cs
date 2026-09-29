// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Diagnostics;

namespace Continuum;

/// <summary>Process identifier provider that uses the current operating system process id.</summary>
public class CurrentProcessIdProvider : IProcessIdProvider
{
    /// <inheritdoc />
    public byte[] GetProcessId()
    {
        var processId = BitConverter.GetBytes(Process.GetCurrentProcess().Id);

        return processId.Length < 2 
            ? throw new InvalidOperationException("Current Process Id is of insufficient length") 
            : processId;
    }
}
