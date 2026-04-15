// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.DotNet.XHarness.Common.CLI;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.CLI;

/// <summary>
/// Round 3 (attacker: secrets in argv echoed to CI logs).
/// </summary>
public class IssuedCommandRedactionTests
{
    [Fact]
    public void FormatArgumentsForLog_RedactsTokenLikeFlags()
    {
        string[] args = { "android", "test", "--access-token=supersecret", "--app=/a.apk" };
        string line = IssuedCommandRedaction.FormatArgumentsForLog(args);
        Assert.Contains("--access-token=[REDACTED]", line);
        Assert.DoesNotContain("supersecret", line);
        Assert.Contains("--app=/a.apk", line);
    }

    [Fact]
    public void FormatArgumentsForLog_LeavesBenignFlags()
    {
        string[] args = { "help", "--output-directory=/tmp/out" };
        string line = IssuedCommandRedaction.FormatArgumentsForLog(args);
        Assert.Contains("--output-directory=/tmp/out", line);
    }

    [Fact]
    public void FormatArgumentsForLog_HandlesNullOrEmpty()
    {
        Assert.Equal(string.Empty, IssuedCommandRedaction.FormatArgumentsForLog(null));
        Assert.Equal(string.Empty, IssuedCommandRedaction.FormatArgumentsForLog(System.Array.Empty<string>()));
    }
}
