// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Net.NetworkInformation;

namespace Continuum;

/// <summary>Worker identifier provider that uses the MAC address of a physical network adapter.</summary>
public class NetworkAddressWorkerIdProvider : IWorkerIdProvider
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No usable network adapter was found at <paramref name="index"/>.</exception>
    public byte[] GetWorkerId(int index)
    {
        return GetNetworkAddress(index);
    }

    static byte[] GetNetworkAddress(int index)
    {
        var network = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                || x.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                || x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx
                || x.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT)
            .Select(x => x.GetPhysicalAddress())
            .Where(x => x != null)
            .Select(x => x.GetAddressBytes())
            .Where(x => x.Length == 6)
            .Skip(index)
            .FirstOrDefault();

        return network == null 
            ? throw new InvalidOperationException("Unable to find usable network adapter for unique address") 
            : network;
    }
}
