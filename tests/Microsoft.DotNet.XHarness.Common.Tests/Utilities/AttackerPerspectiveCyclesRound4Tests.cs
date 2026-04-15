// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Fourth batch of 45 attacker-view cycles (with <see cref="FilePayloadSecurityTests"/> crash-bound tests = 50 total this round).
/// </summary>
public class AttackerPerspectiveCyclesRound4Tests
{
    public static System.Collections.Generic.IEnumerable<object[]> ResolvedUnderBaseUnix()
    {
        yield return new object[] { "/work/out", "/work/out/bin", false };
        yield return new object[] { "/proj/build", "/proj/build/obj", false };
        yield return new object[] { "/artifacts/x", "/artifacts/x/y", false };
        yield return new object[] { "/jenkins/ws", "/jenkins/ws/1", false };
        yield return new object[] { "/github/runner", "/github/runner/_diag", false };
        yield return new object[] { "/azp/agent", "/azp/agent/_work", false };
        yield return new object[] { "/helix/uploads", "/helix/uploads/logs", false };
        yield return new object[] { "/build/a", "/build/b", true };
        yield return new object[] { "/out/rel", "/out/release", true };
        yield return new object[] { "/pkg/nuget", "/pkg/nugets", true };
        yield return new object[] { "/sdk/8", "/sdk/9", true };
        yield return new object[] { "/mono/6", "/mono/7", true };
        yield return new object[] { "/dotnet/preview", "/dotnet/prev", true };
        yield return new object[] { "/wasm/cache", "/wasm/caches", true };
        yield return new object[] { "/ios/sim", "/ios/simulator", true };
        yield return new object[] { "/mac/sdk", "/mac/sdk2", true };
        yield return new object[] { "/xcode/app", "/xcode/apps", true };
        yield return new object[] { "/adb/tmp", "/adb/temp", true };
        yield return new object[] { "/emu/data", "/emu/dat", true };
        yield return new object[] { "/gradle/c", "/gradle/cache", true };
        yield return new object[] { "/maven/r", "/maven/repo", true };
        yield return new object[] { "/npm/l", "/npm/lib", true };
        yield return new object[] { "/yarn/g", "/yarn/global", true };
        yield return new object[] { "/pnpm/s", "/pnpm/store", true };
        yield return new object[] { "/rush/a", "/rush/b", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseUnix))]
    public void Cycle_Round4_ResolvedPathUnderBase_Unix(string basePath, string candidatePath, bool expectThrow)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        if (expectThrow)
        {
            Assert.Throws<ArgumentException>(() =>
                HostPathSecurity.ThrowIfResolvedPathNotUnderBase(basePath, candidatePath, "p"));
        }
        else
        {
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase(basePath, candidatePath, "p");
        }
    }

    public static System.Collections.Generic.IEnumerable<object[]> ResolvedUnderBaseWindows()
    {
        yield return new object[] { @"G:\xh", @"G:\xh\out", false };
        yield return new object[] { @"G:\xh", @"G:\x", true };
        yield return new object[] { @"H:\root", @"H:\root\sub", false };
        yield return new object[] { @"H:\root", @"H:\roo", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseWindows))]
    public void Cycle_Round4_ResolvedPathUnderBase_Windows(string basePath, string candidatePath, bool expectThrow)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        if (expectThrow)
        {
            Assert.Throws<ArgumentException>(() =>
                HostPathSecurity.ThrowIfResolvedPathNotUnderBase(basePath, candidatePath, "p"));
        }
        else
        {
            HostPathSecurity.ThrowIfResolvedPathNotUnderBase(basePath, candidatePath, "p");
        }
    }

    public static System.Collections.Generic.IEnumerable<object[]> NewUnsafePathEdges()
    {
        yield return new object[] { "up/../down/../.." };
        yield return new object[] { "/chain/a/b/../../../c" };
        yield return new object[] { "win\\..\\lose\\.." };
        yield return new object[] { "///../triple" };
        yield return new object[] { "deep/a/b/c/../../../x" };
        yield return new object[] { "/rooted/../jump" };
        yield return new object[] { "relative/../.." };
        yield return new object[] { "x/../y/../z/../.." };
        yield return new object[] { "/a/../b/../c/../d" };
        yield return new object[] { "folder/../folder/../.." };
        yield return new object[] { "\\glob\\..\\.." };
        yield return new object[] { "m/../n/../o/../p" };
        yield return new object[] { "/etc/../var/../.." };
        yield return new object[] { "stack/../../../unwind" };
        yield return new object[] { "pop/../push/../.." };
    }

    [Theory]
    [MemberData(nameof(NewUnsafePathEdges))]
    public void Cycle_Round4_ThrowIfUnsafeHostPath_NewEdges(string path)
    {
        Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
    }

    [Fact]
    public void Cycle_Round4_FilePayloadCrashBounds_Ordered()
    {
        Assert.True(FilePayloadSecurity.DefaultMaxCrashListFileBytes < FilePayloadSecurity.DefaultMaxCrashReportTextBytes);
        Assert.True(FilePayloadSecurity.DefaultMaxTextFileBytes <= FilePayloadSecurity.DefaultMaxCrashReportTextBytes);
    }
}
