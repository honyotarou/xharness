// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Restricts CORS for the local WASM test server to loopback origins instead of <c>Access-Control-Allow-Origin: *</c>.
/// </summary>
public static class WebServerCorsPolicy
{
    /// <summary>
    /// Returns true for origins suitable for xharness's loopback-only test server (127.0.0.1, localhost, ::1).
    /// Missing/empty origin is allowed for non-browser clients; the literal "null" origin is allowed for opaque/sandboxed pages.
    /// </summary>
    public static bool IsLocalhostTestServerOrigin(string? origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            return true;
        }

        if (string.Equals(origin, "null", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        string host = uri.IdnHost;
        if (string.Equals(host, "127.0.0.1", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stricter than <see cref="IsLocalhostTestServerOrigin"/> for stateful endpoints: rejects missing, empty, and literal <c>null</c> origins
    /// (attacker: sandboxed <c>Origin: null</c> and non-browser clients without a session token).
    /// </summary>
    public static bool IsStrictLoopbackHttpBrowserOrigin(string? origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            return false;
        }

        if (string.Equals(origin, "null", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        string host = uri.IdnHost;
        if (string.Equals(host, "127.0.0.1", StringComparison.Ordinal))
        {
            return true;
        }

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
