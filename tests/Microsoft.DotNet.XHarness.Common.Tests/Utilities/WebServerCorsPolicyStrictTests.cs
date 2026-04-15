// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class WebServerCorsPolicyStrictTests
{
    [Theory]
    [InlineData("http://127.0.0.1:1", true)]
    [InlineData("http://localhost:8080", true)]
    [InlineData("https://[::1]:9", true)]
    [InlineData("", false)]
    [InlineData("null", false)]
    [InlineData("https://evil.example/", false)]
    public void IsStrictLoopbackHttpBrowserOrigin_OnlyHttpLoopback(string origin, bool expected) =>
        Assert.Equal(expected, WebServerCorsPolicy.IsStrictLoopbackHttpBrowserOrigin(origin));

    [Fact]
    public void IsStrictLoopbackHttpBrowserOrigin_Null_IsFalse() =>
        Assert.False(WebServerCorsPolicy.IsStrictLoopbackHttpBrowserOrigin(null));
}
