// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.XHarness.Common.Networking;

/// <summary>
/// Bounds TCP reads to mitigate unbounded memory / disk use from a hostile peer (attacker: DoS via huge payloads).
/// </summary>
public static class TcpStreamLimits
{
    /// <summary>Maximum bytes read for iOS test log streaming over TCP.</summary>
    public const int MaxTestLogStreamBytes = 10 * 1024 * 1024;

    /// <summary>Maximum bytes scanned while waiting for the tunnel &quot;ping&quot; handshake.</summary>
    public const int MaxPingHandshakeScanBytes = 2 * 1024 * 1024;

    /// <summary>
    /// Returns how many bytes from this read may be appended without exceeding <paramref name="maxTotal"/> bytes.
    /// </summary>
    public static int AllowedReadLength(int currentTotal, int readSize, int maxTotal)
    {
        if (readSize <= 0)
        {
            return 0;
        }

        if (currentTotal >= maxTotal)
        {
            return 0;
        }

        long remaining = (long)maxTotal - currentTotal;
        if (readSize <= remaining)
        {
            return readSize;
        }

        return (int)remaining;
    }
}
