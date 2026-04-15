// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.DotNet.XHarness.Common.Utilities;
using Xunit;

namespace Microsoft.DotNet.XHarness.Common.Tests.Utilities;

/// <summary>
/// Fifth batch of 50 attacker-view cycles (crash list reads, path containment, traversal edges).
/// </summary>
public class AttackerPerspectiveCyclesRound5Tests
{
    public static System.Collections.Generic.IEnumerable<object[]> ResolvedUnderBaseUnix()
    {
        yield return new object[] { "/base/a", "/base/a/1", false };
        yield return new object[] { "/base/a", "/base/a/2/3", false };
        yield return new object[] { "/mnt/ssd", "/mnt/ssd/games", false };
        yield return new object[] { "/nvme0n1p1", "/nvme0n1p1/home", false };
        yield return new object[] { "/efi/boot", "/efi/boot/grub", false };
        yield return new object[] { "/boot/efi", "/boot/efi/EFI", false };
        yield return new object[] { "/usr/libexec", "/usr/libexec/x", false };
        yield return new object[] { "/base/a", "/base/ab", true };
        yield return new object[] { "/home/u1", "/home/u2", true };
        yield return new object[] { "/lib64", "/lib65", true };
        yield return new object[] { "/sbin/init", "/sbin/initd", true };
        yield return new object[] { "/kernel/v", "/kernel/vm", true };
        yield return new object[] { "/initrd/old", "/initrd/new", true };
        yield return new object[] { "/firmware/a", "/firmware/b", true };
        yield return new object[] { "/dtb/overlays", "/dtb/overlay", true };
        yield return new object[] { "/overlay/work", "/overlay/upper", true };
        yield return new object[] { "/container/layer", "/container/layers", true };
        yield return new object[] { "/k8s/pod", "/k8s/pods", true };
        yield return new object[] { "/docker/v", "/docker/vol", true };
        yield return new object[] { "/podman/r", "/podman/root", true };
        yield return new object[] { "/lxc/c", "/lxc/ct", true };
        yield return new object[] { "/vm/share", "/vm/shares", true };
        yield return new object[] { "/qemu/a", "/qemu/b", true };
        yield return new object[] { "/kvm/guest", "/kvm/guests", true };
        yield return new object[] { "/xen/dom", "/xen/domu", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseUnix))]
    public void Cycle_Round5_ResolvedPathUnderBase_Unix(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { @"J:\vm", @"J:\vm\disk", false };
        yield return new object[] { @"J:\vm", @"J:\vn", true };
        yield return new object[] { @"K:\share", @"K:\share\pub", false };
        yield return new object[] { @"K:\share", @"K:\shar", true };
    }

    [Theory]
    [MemberData(nameof(ResolvedUnderBaseWindows))]
    public void Cycle_Round5_ResolvedPathUnderBase_Windows(string basePath, string candidatePath, bool expectThrow)
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
        yield return new object[] { "loop/../loop/../.." };
        yield return new object[] { "/sys/../proc/../.." };
        yield return new object[] { "dev/../etc/../.." };
        yield return new object[] { "bin/../sbin/../.." };
        yield return new object[] { "lib/../lib64/../.." };
        yield return new object[] { "/usr/../opt/../.." };
        yield return new object[] { "share/../local/../.." };
        yield return new object[] { "include/../bin/../.." };
        yield return new object[] { "src/../obj/../.." };
        yield return new object[] { "debug/../release/../.." };
        yield return new object[] { "main/../feature/../.." };
        yield return new object[] { "trunk/../branch/../.." };
        yield return new object[] { "head/../tag/../.." };
        yield return new object[] { "fork/../upstream/../.." };
        yield return new object[] { "clone/../origin/../.." };
        yield return new object[] { "pull/../push/../.." };
    }

    [Theory]
    [MemberData(nameof(NewUnsafePathEdges))]
    public void Cycle_Round5_ThrowIfUnsafeHostPath_NewEdges(string path)
    {
        Assert.Throws<ArgumentException>(() => HostPathSecurity.ThrowIfUnsafeHostPath(path, "p"));
    }

    [Fact]
    public void Cycle_Round5_CrashListAndReportConstants_AreOrdered()
    {
        Assert.True(FilePayloadSecurity.DefaultMaxCrashListFileBytes > 0);
        Assert.True(FilePayloadSecurity.DefaultMaxCrashReportTextBytes > FilePayloadSecurity.DefaultMaxCrashListFileBytes);
    }

    [Fact]
    public void Cycle_Round5_DefaultTextFileFitsUnderCrashReportCap()
    {
        Assert.True(FilePayloadSecurity.DefaultMaxTextFileBytes <= FilePayloadSecurity.DefaultMaxCrashReportTextBytes);
    }

    [Fact]
    public void Cycle_Round5_IsResolvedPathUnderBaseDirectory_Self()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Assert.True(HostPathSecurity.IsResolvedPathUnderBaseDirectory("/same", "/same"));
    }

    [Fact]
    public void Cycle_Round5_ThrowIfResolvedPathNotUnderBase_SeparatorPrefix()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        HostPathSecurity.ThrowIfResolvedPathNotUnderBase("/tmp/r5", "/tmp/r5/sub", "p");
    }

    [Fact]
    public void Cycle_Round5_FilePayload_Defaults_Distinct()
    {
        Assert.NotEqual(FilePayloadSecurity.DefaultMaxTextFileBytes, FilePayloadSecurity.DefaultMaxCrashListFileBytes);
    }
}
