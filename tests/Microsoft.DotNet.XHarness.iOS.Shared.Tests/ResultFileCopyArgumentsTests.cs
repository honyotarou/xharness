// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.iOS.Shared;
using Xunit;

namespace Microsoft.DotNet.XHarness.iOS.Shared.Tests;

public class ResultFileCopyArgumentsTests
{
    [Fact]
    public void XcrunDevicectlCopyFrom_ProducesArgvWithoutShellConcatenation()
    {
        string[] args = ResultFileCopyArguments.XcrunDevicectlCopyFrom(
            "DEVICE-UDID",
            "/Documents/test-results.xml",
            "/tmp/out.xml",
            "com.contoso.tests");

        Assert.Equal(ResultFileCopyArguments.XcrunExecutable, "/usr/bin/xcrun");
        Assert.Equal("devicectl", args[0]);
        Assert.Equal("--device", args[4]);
        Assert.Equal("DEVICE-UDID", args[5]);
        Assert.Equal("--domain-identifier", args[args.Length - 2]);
        Assert.Equal("com.contoso.tests", args[args.Length - 1]);
    }

    [Fact]
    public void XcrunDevicectlCopyFrom_ShellMetacharactersStayInSingleArgument()
    {
        const string malicious = "com.evil; rm -rf /";
        string[] args = ResultFileCopyArguments.XcrunDevicectlCopyFrom(
            "udid",
            "/Documents/a.xml",
            "/tmp/out",
            malicious);

        int domainIdx = Array.IndexOf(args, "--domain-identifier");
        Assert.True(domainIdx >= 0);
        Assert.Equal(malicious, args[domainIdx + 1]);
        Assert.Single(args, malicious);
    }

    [Fact]
    public void XcrunSimctlGetAppContainer_PassesIdentifiersAsSeparateArguments()
    {
        string[] args = ResultFileCopyArguments.XcrunSimctlGetAppContainer("SIM-UDID", "com.app.bundle");
        Assert.Equal(new[] { "simctl", "get_app_container", "SIM-UDID", "com.app.bundle", "data" }, args);
    }

    [Fact]
    public void CombineSimulatorContainerPath_JoinsPaths()
    {
        string full = ResultFileCopyArguments.CombineSimulatorContainerPath(
            "/Users/x/Library/Developer/CoreSimulator/Devices/X/data/ABC",
            "/Documents/test-results.xml");
        Assert.Equal("/Users/x/Library/Developer/CoreSimulator/Devices/X/data/ABC/Documents/test-results.xml", full);
    }

    [Fact]
    public void CombineSimulatorContainerPath_TrimsTrailingSlashOnContainer()
    {
        string full = ResultFileCopyArguments.CombineSimulatorContainerPath(
            "/path/to/data/",
            "/Documents/test-results.xml");
        Assert.Equal("/path/to/data/Documents/test-results.xml", full);
    }

    [Fact]
    public void CombineSimulatorContainerPath_ThrowsWhenRelativeDoesNotStartWithSlash()
    {
        Assert.Throws<ArgumentException>(() =>
            ResultFileCopyArguments.CombineSimulatorContainerPath("/c", "Documents/x.xml"));
    }
}
