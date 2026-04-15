// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// TDD: stateful localhost endpoints require loopback Origin or a shared session secret.
/// </summary>
public class WebServerStatefulEndpointPolicyTests
{
    private const string Token = "AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_ab";

    [Fact]
    public void IsHttpRequestAllowed_LoopbackOrigin_NoTokenNeeded() =>
        Assert.True(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            hasConflictingOriginHeaders: false,
            hasConflictingStatefulSessionHeaders: false,
            originHeader: "http://127.0.0.1:55",
            sessionHeaderValue: null,
            sessionCookieValue: null,
            expectedSessionToken: Token));

    [Fact]
    public void IsHttpRequestAllowed_NullOrigin_RequiresSessionHeader() =>
        Assert.False(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            false,
            false,
            null,
            null,
            null,
            Token));

    [Fact]
    public void IsHttpRequestAllowed_MissingOrigin_WithValidSession_Allows() =>
        Assert.True(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            false,
            false,
            null,
            Token,
            null,
            Token));

    [Fact]
    public void IsHttpRequestAllowed_LiteralNullOrigin_RequiresSession() =>
        Assert.False(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            false,
            false,
            "null",
            null,
            null,
            Token));

    [Fact]
    public void IsHttpRequestAllowed_DuplicateOrigin_Denies() =>
        Assert.False(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            true,
            false,
            "http://127.0.0.1:1",
            Token,
            null,
            Token));

    [Fact]
    public void IsHttpRequestAllowed_DuplicateSessionHeader_Denies() =>
        Assert.False(WebServerStatefulEndpointPolicy.IsHttpRequestAllowed(
            false,
            true,
            null,
            Token,
            null,
            Token));

    [Fact]
    public void IsWebSocketRequestAllowed_QueryToken_WhenNoOrigin() =>
        Assert.True(WebServerStatefulEndpointPolicy.IsWebSocketRequestAllowed(
            false,
            null,
            Token,
            Token));

    [Fact]
    public void IsWebSocketRequestAllowed_WrongQuery_Denies() =>
        Assert.False(WebServerStatefulEndpointPolicy.IsWebSocketRequestAllowed(
            false,
            null,
            "wrong",
            Token));
}
