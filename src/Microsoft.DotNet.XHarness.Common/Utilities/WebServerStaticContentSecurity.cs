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

        foreach (string ext in s_blockedExtensions)
        {
            if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
