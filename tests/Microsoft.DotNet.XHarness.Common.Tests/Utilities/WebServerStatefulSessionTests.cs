// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

public class WebServerStatefulSessionTests
{
    [Fact]
    public void GenerateToken_Is43CharBase64Url()
    {
        string t = WebServerStatefulSession.GenerateToken();
        Assert.Equal(43, t.Length);
        Assert.DoesNotContain('+', t);
        Assert.DoesNotContain('/', t);
        Assert.DoesNotContain('=', t);
    }

    [Fact]
    public void FixedTimeEquals_RejectsLengthMismatch() =>
        Assert.False(WebServerStatefulSession.FixedTimeEquals("a", "aa"));

    [Fact]
    public void FixedTimeEquals_AcceptsEqualStrings() =>
        Assert.True(WebServerStatefulSession.FixedTimeEquals("same", "same"));
}
