// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Bounds reading text/config files from disk before loading into memory (attacker: multi‑GB file DoS).
/// </summary>
public static class FilePayloadSecurity
{
    /// <summary>Default upper bound for e.g. WASM error-pattern files (line-based regex list).</summary>
    public const long DefaultMaxTextFileBytes = 10 * 1024 * 1024;

    /// <summary>Upper bound for mlaunch crash-report listing output (path list text file).</summary>
    public const long DefaultMaxCrashListFileBytes = 5 * 1024 * 1024;

    /// <summary>Upper bound for a single crash report .ips text pulled to the host log.</summary>
    public const long DefaultMaxCrashReportTextBytes = 50 * 1024 * 1024;

    public static void ThrowIfFileExceedsMaxBytes(string path, string paramName, long maxBytes = DefaultMaxTextFileBytes)
    {
        HostPathSecurity.ThrowIfUnsafeHostPath(path, nameof(path));
        long len = new FileInfo(path).Length;
        if (len > maxBytes)
        {
            throw new ArgumentException($"File exceeds maximum size ({maxBytes} bytes).", paramName);
        }
    }
}
