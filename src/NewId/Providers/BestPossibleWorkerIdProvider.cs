// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

namespace Continuum;

/// <summary>Worker identifier provider that tries the network adapter MAC address first and falls back to a hash of the host name.</summary>
public class BestPossibleWorkerIdProvider : IWorkerIdProvider
{
    /// <inheritdoc />
    /// <exception cref="AggregateException">No provider could supply a worker identifier.</exception>
    public byte[] GetWorkerId(int index)
    {
        var exceptions = new List<Exception>();

        try
        {
            return new NetworkAddressWorkerIdProvider().GetWorkerId(index);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        try
        {
            return new HostNameHashWorkerIdProvider().GetWorkerId(index);
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }

        throw new AggregateException(exceptions);
    }
}
