// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class WebServerStaticContentSecurityTests
{
    [Theory]
    [InlineData("/secrets/deploy.pem", true)]
    [InlineData("/app/foo.pem", true)]
    [InlineData("/.env", true)]
    [InlineData("/index.html", false)]
    [InlineData("/dotnet.wasm", false)]
    public void ShouldBlockRequestPath_BlocksSensitiveExtensions(string path, bool blocked) =>
        Assert.Equal(blocked, WebServerStaticContentSecurity.ShouldBlockRequestPath(path));
}
