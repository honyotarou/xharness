// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Second batch of 50 attacker-view cycles: base-directory containment (zip-slip class), new traversal edges, API consistency.
/// </summary>
public class AttackerPerspectiveCyclesRound2Tests
{
    public static System.Collections.Generic.IEnumerable<object[]> ResolvedUnderBaseUnix()
    {
        yield return new object[] { "/tmp/xh-base", "/tmp/xh-base/child", false };
        yield return new object[] { "/tmp/xh-base", "/tmp/xh-base/child/deep", false };
        yield return new object[] { "/var/tmp/a", "/var/tmp/a/b", false };
        yield return new object[] { "/usr/local/out", "/usr/local/out/bin", false };
        yield return new object[] { "/opt/app", "/opt/app/", false };
        yield return new object[] { "/home/user", "/home/user/docs", false };
        yield return new object[] { "/a/b/c", "/a/b/c/d", false };
        yield return new object[] { "/tmp/xh-a", "/tmp/xh-b", true };
        yield return new object[] { "/tmp/xh-a", "/tmp/xh-a2", true };
        yield return new object[] { "/var/log", "/var/lol", true };
        yield return new object[] { "/mnt/data", "/mnt/dat", true };
        yield return new object[] { "/srv/www", "/srv/ww", true };
        yield return new object[] { "/app/out", "/app/outside", true };
        yield return new object[] { "/x/y", "/x/z", true };
        yield return new object[] { "/root/a", "/root/b", true };
        yield return new object[] { "/bin", "/boot", true };
        yield return new object[] { "/etc/ssl", "/etc/ssh", true };
        yield return new object[] { "/tmp/prefix", "/tmp/prefix-long", true };
        yield return new object[] { "/u/v", "/u/w", true };
        yield return new object[] { "/one/two", "/one/three", true };
        yield return new object[] { "/alpha", "/beta", true };
        yield return new object[] { "/gamma/delta", "/gamma/epsilon", true };
        yield return new object[] { "/zeta", "/eta", true };
        yield return new object[] { "/iota/kappa", "/iota/lambda", true };
        yield return new object[] { "/mu/nu", "/mu/xi", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseUnix))]
    public void Cycle_Round2_ResolvedPathUnderBase_Unix(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { @"C:\xh-base", @"C:\xh-base\child", false };
        yield return new object[] { @"C:\xh-base", @"C:\xh-a", true };
        yield return new object[] { @"D:\app", @"D:\app\data", false };
        yield return new object[] { @"D:\app", @"D:\ap", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseWindows))]
    public void Cycle_Round2_ResolvedPathUnderBase_Windows(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { "a/./b/../c" };
        yield return new object[] { "../" };
        yield return new object[] { "x/../y" };
        yield return new object[] { "/a/b/c/../../d" };
        yield return new object[] { "\\..\\x" };
        yield return new object[] { "//server/../share" };
        yield return new object[] { "/a/b/../" };
        yield return new object[] { "trail/../end" };
        yield return new object[] { "one/two/../../../three" };
        yield return new object[] { "/x/../y/../z" };
        yield return new object[] { "..\\x\\y" };
        yield return new object[] { "/./../bad" };
        yield return new object[] { "nested/../../../escape" };
        yield return new object[] { "/usr/../bin/../etc" };
        yield return new object[] { "foo/../.." };
        yield return new object[] { "z/../../w" };
        yield return new object[] { "/a/b/c/d/../../../../e" };
        yield return new object[] { "p/../q/../r/../s" };
        yield return new object[] { "/home/../root" };
        yield return new object[] { "rel/../abs" };
    }

    [Theory]
    [MemberData(nameof(NewUnsafePathEdges))]
    public void Cycle_Round2_ThrowIfUnsafeHostPath_NewEdges(string path)
    {
        Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
    }

    [Fact]
    public void Cycle_Round2_IsResolvedPathUnderBaseDirectory_MatchesThrow()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.True(HostPathSecurity.IsResolvedPathUnderBaseDirectory("/tmp/a", "/tmp/a/b"));
        Assert.False(HostPathSecurity.IsResolvedPathUnderBaseDirectory("/tmp/a", "/tmp/b"));
    }
}
