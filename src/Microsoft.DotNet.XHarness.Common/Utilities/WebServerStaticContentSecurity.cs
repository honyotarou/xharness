// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Blocks obvious secret/config extensions from the WASM static file middleware (attacker: accidental placement of keys in content root).
/// </summary>
public static class WebServerStaticContentSecurity
{
    private static readonly string[] s_blockedExtensions =
    {
        ".pem", ".key", ".pfx", ".p12", ".jks", ".kdbx", ".env", ".htpasswd", ".htaccess",
        ".crt", ".cer", ".der", ".asc", ".ssh", ".npmrc", ".pypirc", ".dockercfg",
    };

    /// <summary>
    /// True when the request path should not be served as static content.
    /// </summary>
    public static bool ShouldBlockRequestPath(string path) =>
        ShouldBlockRequestPath(path.AsSpan());

    /// <summary>
    /// True when the request path should not be served as static content.
    /// </summary>
    public static bool ShouldBlockRequestPath(ReadOnlySpan<char> path)
    {
        if (path.IsEmpty)
        {
            return false;
        }

        // Normalize trailing slashes so "/secret.pem/" is treated like "/secret.pem".
        while (!path.IsEmpty && path[^1] == '/')
        {
            path = path[..^1];
        }
        if (path.IsEmpty)
        {
            return false;
        }

        // Only evaluate the last path segment (avoid false positives in directory names).
        int lastSlash = path.LastIndexOf('/');
        ReadOnlySpan<char> lastSegment = lastSlash >= 0 ? path[(lastSlash + 1)..] : path;
        if (lastSegment.IsEmpty)
        {
            return false;
        }

        // Also block common sensitive file basenames regardless of extension.
        if (lastSegment.Equals(".npmrc", StringComparison.OrdinalIgnoreCase) ||
            lastSegment.Equals(".pypirc", StringComparison.OrdinalIgnoreCase) ||
            lastSegment.Equals(".dockercfg", StringComparison.OrdinalIgnoreCase) ||
            lastSegment.Equals(".env", StringComparison.OrdinalIgnoreCase) ||
            lastSegment.StartsWith(".env.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (string ext in s_blockedExtensions)
        {
            // Block if the last segment is exactly "<name><ext>" OR starts with it followed by '.' or '/'
            // (e.g. "server.pem.txt" should be blocked because it contains a secret extension as a suffix segment).
            int idx = lastSegment.LastIndexOf(ext, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                continue;
            }

            int extEnd = idx + ext.Length;
            if (extEnd == lastSegment.Length)
            {
                return true;
            }
            char next = lastSegment[extEnd];
            if (next == '.' || next == '/')
            {
                return true;
            }
        }

        return false;
    }
}
