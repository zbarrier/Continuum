// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Continuum;

/// <summary>Worker identifier provider derived from a SHA-1 hash of the host name.</summary>
public class HostNameHashWorkerIdProvider : IWorkerIdProvider
{
    /// <inheritdoc />
    public byte[] GetWorkerId(int index)
    {
        return GetNetworkAddress();
    }

    static byte[] GetNetworkAddress()
    {
        try
        {
            var hostName = Dns.GetHostName();

            byte[] hash;
            using (var hasher = SHA1.Create())
            {
                hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            }

            var bytes = new byte[6];
            Buffer.BlockCopy(hash, 12, bytes, 0, 6);
            bytes[0] |= 0x80;

            return bytes;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to retrieve hostname", ex);
        }
    }
}
