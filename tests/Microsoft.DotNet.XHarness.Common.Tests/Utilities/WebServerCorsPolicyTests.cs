// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// TDD: WASM test server must not reflect <c>Access-Control-Allow-Origin: *</c> for arbitrary web origins (defense in depth vs drive-by scripts).
/// </summary>
public class WebServerCorsPolicyTests
{
    [Theory]
    [InlineData("http://127.0.0.1:1234")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://127.0.0.1:443")]
    [InlineData("https://localhost:5001")]
    [InlineData("http://[::1]:4000")]
    public void IsLocalhostTestServerOrigin_AllowsLoopbackOrigins(string origin)
    {
        Assert.True(WebServerCorsPolicy.IsLocalhostTestServerOrigin(origin));
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("http://attacker.test:1")]
    [InlineData("http://127.0.0.1.evil.com")]
    [InlineData("http://notlocalhost:80")]
    public void IsLocalhostTestServerOrigin_RejectsNonLoopbackHosts(string origin)
    {
        Assert.False(WebServerCorsPolicy.IsLocalhostTestServerOrigin(origin));
    }

    [Fact]
    public void IsLocalhostTestServerOrigin_AllowsMissingOriginForNonBrowserClients()
    {
        Assert.True(WebServerCorsPolicy.IsLocalhostTestServerOrigin(null));
        Assert.True(WebServerCorsPolicy.IsLocalhostTestServerOrigin(string.Empty));
    }

    [Fact]
    public void IsLocalhostTestServerOrigin_AllowsOpaqueNullOriginString()
    {
        Assert.True(WebServerCorsPolicy.IsLocalhostTestServerOrigin("null"));
    }
}
