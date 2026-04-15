// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Third batch of 45 attacker-view cycles (paired with <see cref="FilePayloadSecurityTests"/> = 50 total this round).
/// </summary>
public class AttackerPerspectiveCyclesRound3Tests
{
    public static System.Collections.Generic.IEnumerable<object[]> ResolvedUnderBaseUnix()
    {
        yield return new object[] { "/dev/shm/base", "/dev/shm/base/sub", false };
        yield return new object[] { "/opt/prefix", "/opt/prefix/lib", false };
        yield return new object[] { "/usr/share/pkg", "/usr/share/pkg/data", false };
        yield return new object[] { "/local/build", "/local/build/out", false };
        yield return new object[] { "/cache/xh", "/cache/xh/layer", false };
        yield return new object[] { "/store/0", "/store/0/nested", false };
        yield return new object[] { "/run/user/1", "/run/user/1/doc", false };
        yield return new object[] { "/snap/core/current", "/snap/core/current/bin", false };
        yield return new object[] { "/dev/shm/a", "/dev/shm/b", true };
        yield return new object[] { "/opt/app1", "/opt/app12", true };
        yield return new object[] { "/var/lib/a", "/var/lib/ab", true };
        yield return new object[] { "/srv/http", "/srv/https", true };
        yield return new object[] { "/media/usb", "/media/usbb", true };
        yield return new object[] { "/data/app", "/data/ap", true };
        yield return new object[] { "/sys/fs", "/sys/fx", true };
        yield return new object[] { "/proc/1", "/proc/2", true };
        yield return new object[] { "/net/a", "/net/b", true };
        yield return new object[] { "/disk/one", "/disk/on", true };
        yield return new object[] { "/pool/a", "/pool/b", true };
        yield return new object[] { "/raid/x", "/raid/y", true };
        yield return new object[] { "/vault/k", "/vault/l", true };
        yield return new object[] { "/bucket/p", "/bucket/q", true };
        yield return new object[] { "/stage/dev", "/stage/prod", true };
        yield return new object[] { "/ci/build", "/ci/built", true };
        yield return new object[] { "/qa/out", "/qa/outer", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseUnix))]
    public void Cycle_Round3_ResolvedPathUnderBase_Unix(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { @"E:\build", @"E:\build\bin", false };
        yield return new object[] { @"E:\build", @"E:\bulk", true };
        yield return new object[] { @"F:\data", @"F:\data\x", false };
        yield return new object[] { @"F:\data", @"F:\dat", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseWindows))]
    public void Cycle_Round3_ResolvedPathUnderBase_Windows(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { "x/y/../..\\.." };
        yield return new object[] { "//..//x" };
        yield return new object[] { "a/.././b" };
        yield return new object[] { "/./../y" };
        yield return new object[] { "c/d/e/../../f" };
        yield return new object[] { "..\\..\\z" };
        yield return new object[] { "/w/../x/../y" };
        yield return new object[] { "mix/../..\\back" };
        yield return new object[] { "/up/down/../.." };
        yield return new object[] { "layer/../../../../t" };
        yield return new object[] { "/g/h/i/j/../../../k" };
        yield return new object[] { "dots/../.." };
        yield return new object[] { "path/..\\mixed" };
        yield return new object[] { "/escape/../hole/../.." };
        yield return new object[] { "r/s/t/u/v/../../../w" };
    }

    [Theory]
    [MemberData(nameof(NewUnsafePathEdges))]
    public void Cycle_Round3_ThrowIfUnsafeHostPath_NewEdges(string path)
    {
        Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
    }

    [Fact]
    public void Cycle_Round3_FilePayloadDefault_IsDocumented()
    {
        Assert.InRange(FilePayloadSecurity.DefaultMaxTextFileBytes, 1024 * 1024, long.MaxValue);
    }
}
