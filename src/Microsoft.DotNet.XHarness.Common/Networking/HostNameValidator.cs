// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

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
        return Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }
}
