// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Net;

namespace Microsoft.DotNet.XHarness.Common.Networking;

/// <summary>
/// Resolves which local address TCP test listeners bind to.
/// </summary>
public static class TcpListenerAddressResolver
{
    /// <summary>
    /// Environment variable: when true or 1, listeners bind to loopback only (127.0.0.1 / ::1),
    /// reducing exposure on shared networks. Unset or whitespace-only defaults to <see cref="IPAddress.Loopback"/>
    /// (secure-by-default). Set to false or 0 to bind to <see cref="IPAddress.Any"/> when physical devices on the LAN
    /// must reach the host. Unrecognized non-empty values fail closed to loopback (attacker: typos must not widen bind to LAN).
    /// </summary>
    public const string BindLoopbackOnlyVariableName = "XHARNESS_TCP_BIND_LOOPBACK_ONLY";

    public static IPAddress GetListenAddress()
    {
        string? v = Environment.GetEnvironmentVariable(BindLoopbackOnlyVariableName);
        if (string.IsNullOrWhiteSpace(v))
        {
            return IPAddress.Loopback;
        }

        v = v.Trim();

        if (bool.TryParse(v, out bool b))
        {
            return b ? IPAddress.Loopback : IPAddress.Any;
        }

        if (string.Equals(v, "1", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Loopback;
        }

        if (string.Equals(v, "0", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Any;
        }

        return IPAddress.Loopback;
    }

    /// <summary>
    /// Host segment for <see cref="System.Net.HttpListener"/> prefix URLs (<c>http://{host}:{port}/</c>),
    /// aligned with <see cref="GetListenAddress"/> so HTTP test log listeners match TCP binding policy.
    /// </summary>
    public static string GetHttpListenerPrefixHost()
    {
        return GetListenAddress().Equals(IPAddress.Loopback) ? "127.0.0.1" : "*";
    }
}
