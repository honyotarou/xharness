// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.XHarness.Common.Utilities;

/// <summary>
/// Authorization for <c>POST /test-results</c> and <c>WebSocket /console</c> (TDD surface for localhost hardening).
/// </summary>
public static class WebServerStatefulEndpointPolicy
{
    /// <summary>
    /// Allows loopback browser <c>Origin</c>, or a matching session header when <c>Origin</c> is absent/non-browser.
    /// </summary>
    public static bool IsHttpRequestAllowed(
        bool hasConflictingOriginHeaders,
        bool hasConflictingStatefulSessionHeaders,
        string? originHeader,
        string? sessionHeaderValue,
        string? expectedSessionToken)
    {
        if (hasConflictingOriginHeaders || hasConflictingStatefulSessionHeaders)
        {
            return false;
        }

        if (WebServerCorsPolicy.IsStrictLoopbackHttpBrowserOrigin(originHeader))
        {
            return true;
        }

        if (string.IsNullOrEmpty(expectedSessionToken))
        {
            return false;
        }

        return WebServerStatefulSession.FixedTimeEquals(sessionHeaderValue, expectedSessionToken);
    }

    /// <summary>
    /// WebSocket clients cannot always set custom headers; allow loopback <c>Origin</c> or <c>?xharness_session=</c> token.
    /// </summary>
    public static bool IsWebSocketRequestAllowed(
        bool hasConflictingOriginHeaders,
        string? originHeader,
        string? querySessionValue,
        string? expectedSessionToken)
    {
        if (hasConflictingOriginHeaders)
        {
            return false;
        }

        if (WebServerCorsPolicy.IsStrictLoopbackHttpBrowserOrigin(originHeader))
        {
            return true;
        }

        if (string.IsNullOrEmpty(expectedSessionToken))
        {
            return false;
        }

        return WebServerStatefulSession.FixedTimeEquals(querySessionValue, expectedSessionToken);
    }
}
