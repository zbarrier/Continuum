// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;
/// <summary>Provides worker identifier bytes used to make generated identifiers unique per machine.</summary>
public interface IWorkerIdProvider
{
    /// <summary>Gets the worker identifier bytes.</summary>
    /// <param name="index">The index of the worker source to use (for example, the network adapter index).</param>
    /// <returns>Six bytes identifying the worker.</returns>
    byte[] GetWorkerId(int index);
}
