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
    public void FormatArgumentsForLog_RedactsSpaceSeparatedTokenValueForm()
    {
        string[] args = { "android", "test", "--auth-token", "SuperSecret123", "--app=/a.apk" };
        string line = IssuedCommandRedaction.FormatArgumentsForLog(args);
        Assert.Contains("--auth-token", line);
        Assert.Contains("[REDACTED]", line);
        Assert.DoesNotContain("SuperSecret123", line);
        Assert.Contains("--app=/a.apk", line);
    }

    [Theory]
    [InlineData("--private-key=RSA_PRIVATE_KEY_HERE")]
    [InlineData("--passphrase=MyP@ssphrase")]
    [InlineData("--connection-string=Server=...;Pwd=...")]
    [InlineData("--signing-cert=base64data")]
    public void FormatArgumentsForLog_RedactsAdditionalSensitiveKeyFamilies(string arg)
    {
        string[] args = { "cmd", arg };
        string line = IssuedCommandRedaction.FormatArgumentsForLog(args);
        Assert.Contains("[REDACTED]", line);
        Assert.DoesNotContain("RSA_PRIVATE_KEY_HERE", line);
        Assert.DoesNotContain("MyP@ssphrase", line);
        Assert.DoesNotContain("Pwd=", line);
        Assert.DoesNotContain("base64data", line);
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
