// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;

namespace Microsoft.DotNet.XHarness.Common.Networking;

/// <summary>
/// Validates host strings used for TCP client connections (rejects command-injection style hostnames).
/// </summary>
public static class HostNameValidator
{
    public static bool IsSafeHostName(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        host = host.Trim();
        var t = Uri.CheckHostName(host);
        if (t == UriHostNameType.Unknown)
        {
            return false;
        }

        // Reject obvious local destinations to reduce SSRF risk when this value is later used for connections.
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (string.Equals(host, "metadata.google.internal", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IPAddress.TryParse(host, out var ip))
        {
            if (IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.Any) || ip.Equals(IPAddress.IPv6Any))
            {
                return false;
            }

            // IPv4 private/link-local and well-known metadata.
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                // 10.0.0.0/8
                if (b[0] == 10) return false;
                // 127.0.0.0/8 (loopback handled above, but keep explicit)
                if (b[0] == 127) return false;
                // 169.254.0.0/16 (link-local, includes 169.254.169.254)
                if (b[0] == 169 && b[1] == 254) return false;
                // 172.16.0.0/12
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;
                // 192.168.0.0/16
                if (b[0] == 192 && b[1] == 168) return false;
            }

            // IPv6: loopback handled; also reject link-local/site-local/unique-local/mapped loopback.
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast)
                {
                    return false;
                }
                // fc00::/7 (unique local)
                var bytes = ip.GetAddressBytes();
                if ((bytes[0] & 0xFE) == 0xFC)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
