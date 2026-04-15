// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Http;
using Microsoft.DotNet.XHarness.CLI.Commands.LoopbackTestServer;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.CLI.Tests.Commands;

/// <summary>
/// TDD: loopback test server CORS vs stateful endpoint policy.
/// </summary>
public class WebServerLocalhostOriginPolicyTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("http://127.0.0.1:1234", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("https://evil.example/", false)]
    [InlineData("http://attacker.test:1", false)]
    public void CorsOriginPolicy_MatchesLoopbackPolicy(string? origin, bool expected)
    {
        Assert.Equal(expected, LocalhostStatefulEndpointGate.IsCorsOriginAllowed(origin));
        Assert.Equal(expected, WebServerCorsPolicy.IsLocalhostTestServerOrigin(origin));
    }

    [Fact]
    public void GetOriginHeader_ReturnsFirstOriginOrNull()
    {
        var missing = new DefaultHttpContext();
        Assert.Null(LocalhostStatefulEndpointGate.GetOriginHeader(missing.Request));

        var withOrigin = new DefaultHttpContext();
        withOrigin.Request.Headers.Origin = "https://evil.example";
        Assert.Equal("https://evil.example", LocalhostStatefulEndpointGate.GetOriginHeader(withOrigin.Request));
    }

    [Fact]
    public void HasConflictingOriginHeaders_TrueWhenMultipleOriginValues()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.Append("Origin", "http://127.0.0.1:1");
        ctx.Request.Headers.Append("Origin", "https://evil.example");
        Assert.True(LocalhostStatefulEndpointGate.HasConflictingOriginHeaders(ctx.Request));
    }

    [Fact]
    public void HasConflictingOriginHeaders_FalseWhenSingleOrMissing()
    {
        var none = new DefaultHttpContext();
        Assert.False(LocalhostStatefulEndpointGate.HasConflictingOriginHeaders(none.Request));

        var one = new DefaultHttpContext();
        one.Request.Headers.Origin = "http://127.0.0.1:1";
        Assert.False(LocalhostStatefulEndpointGate.HasConflictingOriginHeaders(one.Request));
    }

    [Fact]
    public void IsHttpRequestAllowed_LoopbackOriginWithoutToken()
    {
        var loopback = new DefaultHttpContext();
        loopback.Request.Headers.Origin = "http://127.0.0.1:9";
        Assert.True(LocalhostStatefulEndpointGate.IsHttpRequestAllowed(loopback.Request, "ignored-token-here"));
    }

    [Fact]
    public void IsHttpRequestAllowed_NoOriginRequiresSession()
    {
        var noOrigin = new DefaultHttpContext();
        Assert.False(LocalhostStatefulEndpointGate.IsHttpRequestAllowed(noOrigin.Request, "tok"));

        noOrigin.Request.Headers[WebServerStatefulSession.HeaderName] = "tok";
        Assert.True(LocalhostStatefulEndpointGate.IsHttpRequestAllowed(noOrigin.Request, "tok"));
    }

    [Fact]
    public void IsHttpRequestAllowed_RejectsEvilOrDuplicateOrigin()
    {
        var evil = new DefaultHttpContext();
        evil.Request.Headers.Origin = "https://evil.example";
        Assert.False(LocalhostStatefulEndpointGate.IsHttpRequestAllowed(evil.Request, "tok"));

        var dup = new DefaultHttpContext();
        dup.Request.Headers.Append("Origin", "http://127.0.0.1:1");
        dup.Request.Headers.Append("Origin", "https://evil.example");
        Assert.False(LocalhostStatefulEndpointGate.IsHttpRequestAllowed(dup.Request, "tok"));
    }

    [Fact]
    public void IsWebSocketRequestAllowed_QuerySessionWhenNoOrigin()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString($"?{WebServerStatefulSession.QueryParameterName}=abc");
        Assert.True(LocalhostStatefulEndpointGate.IsWebSocketRequestAllowed(ctx.Request, "abc"));
    }
}
