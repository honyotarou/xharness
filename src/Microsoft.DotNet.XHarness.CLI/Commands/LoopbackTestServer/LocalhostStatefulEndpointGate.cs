// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Microsoft.Net.Http.Headers;

namespace Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;

/// <summary>
/// Encapsulates HTTP request inspection for loopback stateful endpoints (POST /test-results, WebSocket /console).
/// Keeps ASP.NET-specific parsing out of <see cref="WebServer"/> and policy types in Common.
/// </summary>
internal static class LocalhostStatefulEndpointGate
{
    /// <summary>CORS policy delegate: broad localhost origins for static assets.</summary>
    public static bool IsCorsOriginAllowed(string? originHeader) =>
        WebServerCorsPolicy.IsLocalhostTestServerOrigin(originHeader);

    public static string? GetOriginHeader(HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderNames.Origin, out var originValues) && originValues.Count > 0)
        {
            return originValues[0];
        }

        return null;
    }

    public static bool HasConflictingOriginHeaders(HttpRequest request) =>
        request.Headers.TryGetValue(HeaderNames.Origin, out var originValues) && originValues.Count > 1;

    public static bool HasConflictingStatefulSessionHeaders(HttpRequest request) =>
        request.Headers.TryGetValue(WebServerStatefulSession.HeaderName, out var values) && values.Count > 1;

    public static string? GetStatefulSessionHeader(HttpRequest request)
    {
        if (request.Headers.TryGetValue(WebServerStatefulSession.HeaderName, out var values) && values.Count > 0)
        {
            return values[0];
        }

        return null;
    }

    public static string? GetWebSocketSessionQuery(HttpRequest request) =>
        request.Query.TryGetValue(WebServerStatefulSession.QueryParameterName, out var q) && q.Count > 0 ? q[0] : null;

    public static bool IsHttpRequestAllowed(HttpRequest request, string? statefulSessionToken) =>
        WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            HasConflictingOriginHeaders(request),
            HasConflictingStatefulSessionHeaders(request),
            GetOriginHeader(request),
            GetStatefulSessionHeader(request),
            statefulSessionToken);

    public static bool IsWebSocketRequestAllowed(HttpRequest request, string? statefulSessionToken) =>
        WebServerStatefulEndpointPolicy.IsWebSocketRequestAllowed(
            HasConflictingOriginHeaders(request),
            GetOriginHeader(request),
            GetWebSocketSessionQuery(request),
            statefulSessionToken);
}
