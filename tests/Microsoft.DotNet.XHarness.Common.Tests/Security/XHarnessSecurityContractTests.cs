// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Security;

/// <summary>
/// Cross-cutting security invariants that do not mutate process-wide environment (avoids parallel test races).
/// TCP bind policy: TcpListenerAddressResolverTests; WASM Origin / duplicate Origin: WebServerLocalhostOriginPolicyTests (CLI).
/// </summary>
public class XHarnessSecurityContractTests
{
    [Fact]
    public void Contract_CorsPolicy_ExternalOriginRejected()
    {
        Assert.False(WebServerCorsPolicy.IsLocalhostTestServerOrigin("https://malicious.example/"));
    }

    [Fact]
    public void Contract_CorsPolicy_LoopbackOriginAccepted()
    {
        Assert.True(WebServerCorsPolicy.IsLocalhostTestServerOrigin("http://127.0.0.1:5000"));
    }

    [Fact]
    public void Contract_HostPath_TraversalRejected()
    {
        Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath("/safe/../etc", "p"));
    }

    [Fact]
    public void Contract_HostPath_ResolvedContainment_MatchesUnixPrefixSemantics()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.True(HostPathSecurity.IsResolvedPathUnderBaseDirectory("/tmp/jail", "/tmp/jail/sub/file"));
        Assert.False(HostPathSecurity.IsResolvedPathUnderBaseDirectory("/tmp/jail", "/tmp/other"));
    }
}
